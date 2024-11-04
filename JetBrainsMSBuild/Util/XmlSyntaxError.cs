using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace JetBrainsMSBuild.Util
{
    public class XmlSyntaxError
    {
        private readonly string filePath;
        public XmlSyntaxError(string _filePath)
        {
            filePath = _filePath;
        }

        public void HighlightSyntaxErrors(RichTextBox richTextBox, XmlException ex)
        {
            // Display XML content in the RichTextBox
            string xmlContent = File.ReadAllText(filePath);
            DisplayXmlContent(richTextBox, xmlContent);

            // Highlight the line with the syntax error
            HighlightErrorLine(richTextBox, ex.LineNumber, ex.LinePosition);

            // Display the error message in a MessageBox
            MessageBox.Show($"Syntax error in XML: {ex.Message}", "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DisplayXmlContent(RichTextBox richTextBox, string content)
        {
            richTextBox.Clear();
            richTextBox.Text = content;
            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;

            // Enable both vertical and horizontal scroll bars
            richTextBox.ScrollBars = RichTextBoxScrollBars.Both;

            // Disable word wrap to allow horizontal scrolling
            richTextBox.WordWrap = false;
        }

        private void HighlightErrorLine(RichTextBox richTextBox, int lineNumber, int linePosition)
        {
            // Adjust for zero-based index in RichTextBox
            int adjustedLineNumber = lineNumber - 1;

            // Get the starting character index of the error line
            int lineStartIndex = richTextBox.GetFirstCharIndexFromLine(adjustedLineNumber);

            // Calculate the exact character index by adding the line position
            int errorCharIndex = lineStartIndex + (linePosition - 1);

            // Select the error position and highlight the entire line
            richTextBox.Select(lineStartIndex, richTextBox.Lines[adjustedLineNumber].Length);
            richTextBox.SelectionColor = Color.Red;

            // Move the cursor to the error position for visibility
            richTextBox.SelectionStart = errorCharIndex;
            richTextBox.ScrollToCaret();
        }
    }
}
