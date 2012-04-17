namespace BinaryFileSchemaGUI
{
	partial class BFSGUI
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BFSGUI));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolNewSchema = new System.Windows.Forms.ToolStripButton();
            this.toolOpenSchema = new System.Windows.Forms.ToolStripButton();
            this.toolSaveSchema = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolCompileSchema = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolComboLanguage = new System.Windows.Forms.ToolStripComboBox();
            this.toolGenerateCode = new System.Windows.Forms.ToolStripButton();
            this.toolAbout = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.richTextBox = new BinaryFileSchemaGUI.FastRichEdit();
            this.listViewErrorBox = new System.Windows.Forms.ListView();
            this.columnMessage = new System.Windows.Forms.ColumnHeader();
            this.imageListErrorIcons = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeBFSstructure = new System.Windows.Forms.TreeView();
            this.imgListBlocks = new System.Windows.Forms.ImageList(this.components);
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.toolStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolNewSchema,
            this.toolOpenSchema,
            this.toolSaveSchema,
            this.toolStripSeparator1,
            this.toolCompileSchema,
            this.toolStripSeparator2,
            this.toolComboLanguage,
            this.toolGenerateCode,
            this.toolAbout});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(720, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolNewSchema
            // 
            this.toolNewSchema.Image = global::BinaryFileSchemaGUI.Properties.Resources.page;
            this.toolNewSchema.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolNewSchema.Name = "toolNewSchema";
            this.toolNewSchema.Size = new System.Drawing.Size(51, 22);
            this.toolNewSchema.Text = "New";
            this.toolNewSchema.ToolTipText = "New Schema";
            this.toolNewSchema.Click += new System.EventHandler(this.toolNewSchema_Click);
            // 
            // toolOpenSchema
            // 
            this.toolOpenSchema.Image = global::BinaryFileSchemaGUI.Properties.Resources.folder;
            this.toolOpenSchema.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolOpenSchema.Name = "toolOpenSchema";
            this.toolOpenSchema.Size = new System.Drawing.Size(56, 22);
            this.toolOpenSchema.Text = "Open";
            this.toolOpenSchema.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolSaveSchema
            // 
            this.toolSaveSchema.Image = global::BinaryFileSchemaGUI.Properties.Resources.disk1;
            this.toolSaveSchema.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolSaveSchema.Name = "toolSaveSchema";
            this.toolSaveSchema.Size = new System.Drawing.Size(51, 22);
            this.toolSaveSchema.Text = "Save";
            this.toolSaveSchema.Click += new System.EventHandler(this.toolSaveSchema_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolCompileSchema
            // 
            this.toolCompileSchema.Image = global::BinaryFileSchemaGUI.Properties.Resources.control_play_blue;
            this.toolCompileSchema.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCompileSchema.Name = "toolCompileSchema";
            this.toolCompileSchema.Size = new System.Drawing.Size(72, 22);
            this.toolCompileSchema.Text = "Compile";
            this.toolCompileSchema.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolComboLanguage
            // 
            this.toolComboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolComboLanguage.DropDownWidth = 64;
            this.toolComboLanguage.Name = "toolComboLanguage";
            this.toolComboLanguage.Size = new System.Drawing.Size(75, 25);
            // 
            // toolGenerateCode
            // 
            this.toolGenerateCode.Enabled = false;
            this.toolGenerateCode.Image = global::BinaryFileSchemaGUI.Properties.Resources.cog_go;
            this.toolGenerateCode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolGenerateCode.Name = "toolGenerateCode";
            this.toolGenerateCode.Size = new System.Drawing.Size(103, 22);
            this.toolGenerateCode.Text = "Generate code";
            this.toolGenerateCode.Click += new System.EventHandler(this.toolGenerateCode_Click);
            // 
            // toolAbout
            // 
            this.toolAbout.Image = global::BinaryFileSchemaGUI.Properties.Resources.help;
            this.toolAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolAbout.Name = "toolAbout";
            this.toolAbout.Size = new System.Drawing.Size(60, 22);
            this.toolAbout.Text = "About";
            this.toolAbout.ToolTipText = "About";
            this.toolAbout.Visible = false;
            this.toolAbout.Click += new System.EventHandler(this.toolAbout_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "*.fsc";
            this.openFileDialog.Filter = "Binary File Schemas|*.fsc|All files|*.*";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.richTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listViewErrorBox);
            this.splitContainer1.Size = new System.Drawing.Size(525, 430);
            this.splitContainer1.SplitterDistance = 279;
            this.splitContainer1.TabIndex = 2;
            // 
            // richTextBox
            // 
            this.richTextBox.AcceptsTab = true;
            this.richTextBox.AllowPaint = true;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox.ForeColor = System.Drawing.Color.Black;
            this.richTextBox.HideSelection = false;
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(525, 279);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            this.richTextBox.WordWrap = false;
            // 
            // listViewErrorBox
            // 
            this.listViewErrorBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnMessage});
            this.listViewErrorBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewErrorBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewErrorBox.HideSelection = false;
            this.listViewErrorBox.Location = new System.Drawing.Point(0, 0);
            this.listViewErrorBox.MultiSelect = false;
            this.listViewErrorBox.Name = "listViewErrorBox";
            this.listViewErrorBox.ShowGroups = false;
            this.listViewErrorBox.Size = new System.Drawing.Size(525, 147);
            this.listViewErrorBox.SmallImageList = this.imageListErrorIcons;
            this.listViewErrorBox.TabIndex = 1;
            this.listViewErrorBox.UseCompatibleStateImageBehavior = false;
            this.listViewErrorBox.View = System.Windows.Forms.View.Details;
            this.listViewErrorBox.ItemActivate += new System.EventHandler(this.listViewErrorBox_ItemActivate);
            // 
            // columnMessage
            // 
            this.columnMessage.Text = "Message";
            this.columnMessage.Width = 521;
            // 
            // imageListErrorIcons
            // 
            this.imageListErrorIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListErrorIcons.ImageStream")));
            this.imageListErrorIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListErrorIcons.Images.SetKeyName(0, "information.png");
            this.imageListErrorIcons.Images.SetKeyName(1, "error.png");
            this.imageListErrorIcons.Images.SetKeyName(2, "stop.png");
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 25);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.treeBFSstructure);
            this.splitContainer2.Size = new System.Drawing.Size(720, 430);
            this.splitContainer2.SplitterDistance = 525;
            this.splitContainer2.TabIndex = 3;
            // 
            // treeBFSstructure
            // 
            this.treeBFSstructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeBFSstructure.ImageKey = "page_white_text.png";
            this.treeBFSstructure.ImageList = this.imgListBlocks;
            this.treeBFSstructure.Location = new System.Drawing.Point(0, 0);
            this.treeBFSstructure.Name = "treeBFSstructure";
            this.treeBFSstructure.SelectedImageIndex = 0;
            this.treeBFSstructure.Size = new System.Drawing.Size(191, 430);
            this.treeBFSstructure.TabIndex = 0;
            this.treeBFSstructure.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeBFSstructure_AfterSelect);
            // 
            // imgListBlocks
            // 
            this.imgListBlocks.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListBlocks.ImageStream")));
            this.imgListBlocks.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListBlocks.Images.SetKeyName(0, "database.png");
            this.imgListBlocks.Images.SetKeyName(1, "database_go.png");
            this.imgListBlocks.Images.SetKeyName(2, "comments.png");
            this.imgListBlocks.Images.SetKeyName(3, "brick.png");
            this.imgListBlocks.Images.SetKeyName(4, "bullet_blue.png");
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "*.fsc";
            this.saveFileDialog.Filter = "Binary File Schemas|*.fsc|All files|*.*";
            // 
            // BFSGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 455);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BFSGUI";
            this.Text = "Binary File Schema";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolCompileSchema;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStripButton toolOpenSchema;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TreeView treeBFSstructure;
		private System.Windows.Forms.ImageList imgListBlocks;
		private System.Windows.Forms.ListView listViewErrorBox;
		private System.Windows.Forms.ColumnHeader columnMessage;
		private System.Windows.Forms.ImageList imageListErrorIcons;
		private FastRichEdit richTextBox;
		private System.Windows.Forms.ToolStripButton toolNewSchema;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolAbout;
		private System.Windows.Forms.ToolStripButton toolGenerateCode;
		private System.Windows.Forms.ToolStripComboBox toolComboLanguage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolSaveSchema;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
	}
}

