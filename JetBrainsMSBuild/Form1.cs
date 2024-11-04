using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using JetBrainsMSBuild.Util;
using System.Xml;

namespace JetBrainsMSBuild
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML Files (*.XML)|*.xml";
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                if (Path.GetExtension(filePath).ToLower() != ".xml")
                {
                    MessageBox.Show("Please select only XML files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    XDocument xDocument = XDocument.Load(filePath);

                    CsprojValidator validator = new CsprojValidator();
                    List<string> errorList = validator.ValidateStructure(xDocument);

                    XmlHighlighter highlighter = new XmlHighlighter(filePath);
                    highlighter.DisplayColoredXml(richTextBox1);

                    // Display errors if they exist
                    if (errorList.Count > 0)
                    {
                        string errors = string.Join("\n", errorList);
                        MessageBox.Show("Errors found:\n" + errors, "Structure Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("No errors found.", "Validation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (XmlException ex)
                {
                    XmlSyntaxError xmlSyntaxError = new XmlSyntaxError(filePath);
                    xmlSyntaxError.HighlightSyntaxErrors(richTextBox1, ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading file: " + ex.Message);
                }
            }
        }
    }
}
