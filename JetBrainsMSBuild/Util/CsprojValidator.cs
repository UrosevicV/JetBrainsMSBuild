using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace JetBrainsMSBuild.Util
{
    public class CsprojValidator
    {
        private readonly List<string> errorList = new List<string>();
        private readonly List<string> customTasks = new List<string>();

        public List<string> ValidateStructure(XDocument xdoc)
        {
            // Check if the root element is <Project>
            XElement projectElement = xdoc.Root;
            ValidateProjectElement(projectElement);

            // Validate Choose elements within <Project>
            foreach (var chooseElement in projectElement.Elements().Where(e => e.Name.LocalName == "Choose"))
            {
                ValidateChooseElement(chooseElement);
            }

            // Validate Import elements within <Project>
            foreach (var importElement in projectElement.Elements().Where(e => e.Name.LocalName == "Import"))
            {
                ValidateImportElement(importElement);
            }

            // Validate ProjectExtensions elements within <Project>
            foreach (var projectExtensionsElement in projectElement.Elements().Where(e => e.Name.LocalName == "ProjectExtensions"))
            {
                ValidateProjectExtensionsElement(projectExtensionsElement);
            }

            // Validate UsingTask elements within <Project>
            foreach (var usingTaskElement in projectElement.Elements().Where(e => e.Name.LocalName == "UsingTask"))
            {
                ValidateUsingTaskElement(usingTaskElement);
                ExtractCustomTasks(usingTaskElement);
            }

            // Validate Target elements within <Project>
            foreach (var targetElement in projectElement.Elements().Where(e => e.Name.LocalName == "Target"))
            {
                ValidateTargetElement(targetElement);
            }

            // Additional validation will be added later
            return errorList;
        }

        private void ValidateProjectElement(XElement projectElement)
        {
            if (projectElement == null || projectElement.Name.LocalName != "Project")
            {
                errorList.Add("Missing or incorrect root <Project> element.");
            }

            // List of allowed elements within <Project>
            var allowedElements = new HashSet<string>
            {
                "Choose",
                "Import",
                "ItemGroup",
                "ProjectExtensions",
                "PropertyGroup",
                "Target",
                "UsingTask"
            };

            // Check for any disallowed elements within <Project>
            foreach (var element in projectElement.Elements())
            {
                if (!allowedElements.Contains(element.Name.LocalName))
                {
                    errorList.Add($"Disallowed element <{element.Name.LocalName}> found within <Project>.");
                }
            }
        }

        // Method to validate Choose elements
        private void ValidateChooseElement(XElement chooseElement)
        {
            bool hasWhen = false;
            bool foundOtherwise = false;

            foreach (var child in chooseElement.Elements())
            {
                if (child.Name.LocalName == "When")
                {
                    hasWhen = true;
                    if (foundOtherwise)
                    {
                        errorList.Add("<When> element cannot appear after an <Otherwise> element within <Choose>.");
                    }
                }
                else if (child.Name.LocalName == "Otherwise")
                {
                    if (foundOtherwise)
                    {
                        errorList.Add("Only one <Otherwise> element is allowed within <Choose>.");
                    }
                    foundOtherwise = true;
                }
                else
                {
                    errorList.Add($"Disallowed element <{child.Name.LocalName}> found within <Choose>. Only <When> and <Otherwise> are allowed.");
                }
            }

            // Check if there is at least one <When> element
            if (!hasWhen)
            {
                errorList.Add("<Choose> element must contain at least one <When> element.");
            }
        }

        // Method to validate Import elements
        private void ValidateImportElement(XElement importElement)
        {
            // Check if <Import> element has the required "Project" attribute
            XAttribute projectAttribute = importElement.Attribute("Project");
            if (projectAttribute == null || string.IsNullOrWhiteSpace(projectAttribute.Value))
            {
                errorList.Add("<Import> element must have a non-empty 'Project' attribute.");
            }

            // Check for any disallowed nested elements
            if (importElement.HasElements)
            {
                errorList.Add("<Import> element must not contain nested elements.");
            }
        }

        // Method to validate ItemGroup elements
        private void ValidateItemGroupElement(XElement itemGroupElement)
        {
            // Check for any disallowed nested elements within <ItemGroup>
            foreach (var child in itemGroupElement.Elements())
            {
                if (child.Name.LocalName != "Item")
                {
                    errorList.Add($"Disallowed element <{child.Name.LocalName}> found within <ItemGroup>. Only <Item> elements are allowed.");
                }
            }
        }

        // Method to validate ProjectExtensions elements
        private void ValidateProjectExtensionsElement(XElement projectExtensionsElement)
        {
            // Check if there is more than one <ProjectExtensions> element within <Project>
            if (projectExtensionsElement.Elements("ProjectExtensions").Count() > 1)
            {
                errorList.Add("Only one <ProjectExtensions> element is allowed within <Project>.");
            }

            // Check for any nested elements within <ProjectExtensions>
            if (projectExtensionsElement.HasElements)
            {
                errorList.Add("<ProjectExtensions> element must not contain nested elements.");
            }
        }

        // Method to validate PropertyGroup elements
        private void ValidatePropertyGroupElement(XElement propertyGroupElement)
        {
            // Iterate over each child element within <PropertyGroup>
            foreach (var propertyElement in propertyGroupElement.Elements())
            {
                // Each property element should not have nested elements; it should be a simple key-value pair or self-closing tag
                if (propertyElement.HasElements)
                {
                    errorList.Add($"Nested elements are not allowed within property <{propertyElement.Name.LocalName}> inside <PropertyGroup>.");
                }

                // Allow empty values, but check if it has a Condition attribute
                XAttribute conditionAttribute = propertyElement.Attribute("Condition");
                if (string.IsNullOrWhiteSpace(propertyElement.Value) && conditionAttribute == null)
                {
                    errorList.Add($"Property <{propertyElement.Name.LocalName}> in <PropertyGroup> is empty and should either have a non-empty value or a 'Condition' attribute.");
                }
            }
        }


        // Method to validate Target elements
        private void ValidateTargetElement(XElement targetElement)
        {
            // Check if Target element has the required "Name" attribute
            XAttribute nameAttribute = targetElement.Attribute("Name");
            if (nameAttribute == null || string.IsNullOrWhiteSpace(nameAttribute.Value))
            {
                errorList.Add("<Target> element must have a non-empty 'Name' attribute.");
            }

            // List of allowed built-in task elements within <Target>
            var allowedElements = new HashSet<string>
            {
                "PropertyGroup",
                "ItemGroup",
                "Error",
                "Warning",
                "Message",
                "Exec"
            };

            // Validate each child element within <Target>
            foreach (var child in targetElement.Elements())
            {
                if (!allowedElements.Contains(child.Name.LocalName) && !IsCustomTask(child.Name.LocalName))
                {
                    errorList.Add($"Disallowed element <{child.Name.LocalName}> found within <Target>. Allowed elements are standard MSBuild tasks and registered custom tasks.");
                }
            }
        }

        // Helper method to check if an element is a custom task
        private bool IsCustomTask(string elementName)
        {
            // Check if the element name exists in the customTasks set
            return customTasks.Contains(elementName);
        }

        // Method to extract and store custom task names from UsingTask elements
        private void ExtractCustomTasks(XElement usingTaskElement)
        {
            // Retrieve the TaskName attribute
            XAttribute taskNameAttribute = usingTaskElement.Attribute("TaskName");

            // Add the TaskName to customTasks if it is not null or empty
            if (taskNameAttribute != null && !string.IsNullOrWhiteSpace(taskNameAttribute.Value))
            {
                customTasks.Add(taskNameAttribute.Value);
            }
        }

        // Method to validate UsingTask elements
        private void ValidateUsingTaskElement(XElement usingTaskElement)
        {
            // Check if UsingTask element has the required attributes "TaskName" and either "AssemblyFile" or "AssemblyName"
            XAttribute taskNameAttribute = usingTaskElement.Attribute("TaskName");
            XAttribute assemblyFileAttribute = usingTaskElement.Attribute("AssemblyFile");
            XAttribute assemblyNameAttribute = usingTaskElement.Attribute("AssemblyName");

            if (taskNameAttribute == null || string.IsNullOrWhiteSpace(taskNameAttribute.Value))
            {
                errorList.Add("<UsingTask> element must have a non-empty 'TaskName' attribute.");
            }

            if (assemblyFileAttribute == null && assemblyNameAttribute == null)
            {
                errorList.Add("<UsingTask> element must have either 'AssemblyFile' or 'AssemblyName' attribute.");
            }
        }
    }
}
