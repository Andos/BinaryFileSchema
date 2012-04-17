using System;
using System.IO;
using System.Windows.Forms;
using BFSchema;
using Peg.Base;
using System.Diagnostics;

namespace BinaryFileSchemaGUI
{
	public partial class BFSGUI : Form
	{
		public BFSGUI()
		{
			InitializeComponent();
		}

		BinaryFileSchema lastSchema;
        string openFile = string.Empty;

		private void Form1_Load(object sender, EventArgs e)
		{
            /*string filename = @"C:\";
            if (File.Exists(filename))
            {
                LoadGrammar(filename);
                ParseGrammar();
            }*/
			toolComboLanguage.Items.Add(new BFSchema.CodeGenerators.CSharp.CSharpGenerator());
			toolComboLanguage.SelectedIndex = 0;
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			ParseGrammar();
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			DialogResult res = openFileDialog.ShowDialog();
			if (res == DialogResult.OK)
			{
				LoadGrammar(openFileDialog.FileName);
				ParseGrammar();
			}
		}

		private void LoadGrammar( string filename )
		{
            if (!File.Exists(filename))
                return;

			StreamReader reader = new StreamReader(filename);
			string source = reader.ReadToEnd();
			richTextBox.Text = source;
			reader.Close();
			richTextBox.Select(0, 0);
            openFile = filename;
            toolSaveSchema.Enabled = true;
		}

		private void ParseGrammar( )
		{
			richTextBox.AllowPaint = false;
			treeBFSstructure.BeginUpdate();
			listViewErrorBox.BeginUpdate();

			IBfsErrorHandler errorHandler = new ListViewErrorHandler(listViewErrorBox);
			string source = richTextBox.Text;

			listViewErrorBox.Items.Clear();
			treeBFSstructure.Nodes.Clear();

            Stopwatch timer = new Stopwatch();
            timer.Start();
			BinaryFileSchema schema = BfsCompiler.ParseBfs(source, errorHandler);
            timer.Stop();

			if (schema != null)
			{
				errorHandler.HandleMessage("Parsed and processed in " + timer.ElapsedMilliseconds + " milliseconds (" + timer.ElapsedTicks + " ticks)");
				IterateSchema(schema);
				SchemaColorizer.ColorizeSchema(schema, richTextBox);
				toolGenerateCode.Enabled = true;
				lastSchema = schema;
			}
			else
				toolGenerateCode.Enabled = false;

			richTextBox.AllowPaint = true;
			treeBFSstructure.EndUpdate();
			listViewErrorBox.EndUpdate();
		}

		private string GetNodeText(PegNode node, string source)
		{
			return source.Substring(node.match_.posBeg_, node.match_.Length).Trim();
		}

		private void treeBFSstructure_AfterSelect(object sender, TreeViewEventArgs e)
		{
			IBfsSourceNode snode = e.Node.Tag as IBfsSourceNode;

			if (snode != null)
				richTextBox.Select(snode.SourceRange.Begin, snode.SourceRange.Length);
			else
				listViewErrorBox.Items.Add("Error! No range was stored in the nodes!",0);
		}

		private void IterateSchema( BinaryFileSchema schema )
		{
			foreach (IBfsDataBlock block in schema.DatablockList)
			{
				TreeNode node = treeBFSstructure.Nodes.Add(block.Name);
				node.Tag = block;
				node.ImageIndex = node.SelectedImageIndex = 0;

				if (block is IBfsStructType)
				{
					if (block is IBfsConsumptionType)
						node.ImageIndex = node.SelectedImageIndex = 1;

					foreach (BfsStructField field in (block as IBfsStructType).StructFields.Values)
					{
						TreeNode newnode = node.Nodes.Add(field.ToString());
						newnode.Tag = field;
						newnode.ImageIndex = newnode.SelectedImageIndex = 4;
					}
				}
				else if (block is BfsEnum)
				{
					node.ImageIndex = node.SelectedImageIndex = 2;
					foreach (BfsEnumField field in (block as BfsEnum).EnumFields)
					{
						TreeNode newnode = node.Nodes.Add(field.ToString());
						newnode.Tag = field;
						newnode.ImageIndex = newnode.SelectedImageIndex = 4;
					}
				}
				else if (block is BfsBitfield)
				{
					node.ImageIndex = node.SelectedImageIndex = 3;
					foreach (BfsBitfieldField field in (block as BfsBitfield).BitFieldFields)
					{
						TreeNode newnode = node.Nodes.Add(field.ToString());
						newnode.Tag = field;
						newnode.ImageIndex = newnode.SelectedImageIndex = 4;
					}
				}
				node.Expand();
			}
		}

		private void listViewErrorBox_ItemActivate(object sender, EventArgs e)
		{
			if (listViewErrorBox.SelectedItems.Count == 0)
				return;

			if (listViewErrorBox.SelectedItems[0].Tag is SourceError)
			{
				SourceError error = listViewErrorBox.SelectedItems[0].Tag as SourceError;
				richTextBox.Select(error.SourceRange.Begin, error.SourceRange.Length);
				richTextBox.Focus();
				return;
			}
			else
			{
				string message = listViewErrorBox.SelectedItems[0].Text;
				if (message.StartsWith("<"))
				{
					string[] items = message.Split("<,>".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					int line = Math.Max(int.Parse(items[0]) - 1,0);
					int index = int.Parse(items[1]);
					int begin = Math.Max(richTextBox.GetFirstCharIndexFromLine(line), 0);
					int end = Math.Max(richTextBox.GetFirstCharIndexFromLine(line + 1) - begin, 0);
					richTextBox.Select(begin, end);
					richTextBox.Focus();
				}
			}
		}

		private void toolNewSchema_Click(object sender, EventArgs e)
		{
			richTextBox.Clear();
			toolGenerateCode.Enabled = false;
            openFile = string.Empty;
		}

        private void toolAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Binary File Schema - By Anders Riggelsen, www.andersriggelsen.dk", "About Binary File Schema", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

		private void toolGenerateCode_Click(object sender, EventArgs e)
		{
			CodeGenerator generator = toolComboLanguage.SelectedItem as CodeGenerator;
			string sourceCode = generator.GenerateCode(lastSchema);
			CodeOutput codeDialog = new CodeOutput(sourceCode, lastSchema.FormatBlock.Name);
			codeDialog.ShowDialog();
		}

        private void toolSaveSchema_Click(object sender, EventArgs e)
        {
            if (openFile == string.Empty)
            {
                DialogResult res = saveFileDialog.ShowDialog();
                if (res == DialogResult.OK)
                    openFile = saveFileDialog.FileName;
                else
                    return;
            }

            StreamWriter writer = new StreamWriter(openFile);
            writer.Write(richTextBox.Text);
            writer.Close();
        }

	}
}
