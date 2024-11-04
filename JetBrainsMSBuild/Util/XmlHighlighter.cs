using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace JetBrainsMSBuild.Util
{
    public class XmlHighlighter
    {
        private readonly string filePath;
        public XmlHighlighter(string _filePath)
        {
            filePath = _filePath;
        }

        public void DisplayColoredXml(RichTextBox richTextBox)
        {
            string xmlContent = File.ReadAllText(filePath);

            // Clear previous content of the RichTextBox
            richTextBox.Clear();

            // Display the entire text in black
            AppendText(xmlContent, Color.Black, richTextBox);

            // Colorize elements and attributes
            ColorElementsAndAttributes(richTextBox);

            // Move the cursor to the start of the RichTextBox
            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
        }

        private void ColorElementsAndAttributes(RichTextBox richTextBox)
        {
            // Regular expressions for elements, attributes, and values
            var elementRegex = new Regex(@"<(/?[^>\s]+)");
            var attributeRegex = new Regex(@"(\s+[^\s=]+)(?==)");
            var valueRegex = new Regex(@"""([^""]*)""");
            var innerTextRegex = new Regex(@">(.*?)<");

            // Select and color each element tag in dark blue
            ApplyRegexColor(elementRegex, Color.DarkBlue, richTextBox);

            // Select and color each attribute in red
            ApplyRegexColor(attributeRegex, Color.Red, richTextBox);

            // Select and color each attribute value in green
            ApplyRegexColor(valueRegex, Color.Green, richTextBox);

            // Select and bold the inner text between tags
            ApplyRegexBold(innerTextRegex, richTextBox);
        }

        private void ApplyRegexColor(Regex regex, Color color, RichTextBox richTextBox)
        {
            foreach (Match match in regex.Matches(richTextBox.Text))
            {
                richTextBox.Select(match.Index, match.Length);
                richTextBox.SelectionColor = color;
            }
        }

        private void ApplyRegexBold(Regex regex, RichTextBox richTextBox)
        {
            foreach (Match match in regex.Matches(richTextBox.Text))
            {
                if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
                {
                    richTextBox.Select(match.Index + 1, match.Groups[1].Length);
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
                    richTextBox.SelectionColor = Color.Black;
                }
            }
        }

        private void AppendText(string text, Color color, RichTextBox richTextBox)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }
    }
}
