using System;
using System.Windows.Forms;

namespace BinaryFileSchemaGUI
{
	public partial class CodeOutput : Form
	{
        string filename = "Parser";

		public CodeOutput( string sourceCode, string filename )
		{
			InitializeComponent();
			richEditSourceCode.Text = sourceCode;
            this.filename = filename;
		}

        private void toolStripSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = filename + "Parser.cs";
            DialogResult res = saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                richEditSourceCode.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

	}
}
