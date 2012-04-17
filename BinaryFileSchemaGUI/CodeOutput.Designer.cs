namespace BinaryFileSchemaGUI
{
	partial class CodeOutput
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSave = new System.Windows.Forms.ToolStripButton();
            this.richEditSourceCode = new BinaryFileSchemaGUI.FastRichEdit();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSave});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(783, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSave
            // 
            this.toolStripSave.Image = global::BinaryFileSchemaGUI.Properties.Resources.disk;
            this.toolStripSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSave.Name = "toolStripSave";
            this.toolStripSave.Size = new System.Drawing.Size(80, 22);
            this.toolStripSave.Text = "Save code";
            this.toolStripSave.Click += new System.EventHandler(this.toolStripSave_Click);
            // 
            // richEditSourceCode
            // 
            this.richEditSourceCode.AcceptsTab = true;
            this.richEditSourceCode.AllowPaint = true;
            this.richEditSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richEditSourceCode.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richEditSourceCode.Location = new System.Drawing.Point(0, 25);
            this.richEditSourceCode.Name = "richEditSourceCode";
            this.richEditSourceCode.Size = new System.Drawing.Size(783, 445);
            this.richEditSourceCode.TabIndex = 1;
            this.richEditSourceCode.Text = "";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "cs";
            this.saveFileDialog1.Filter = "C# files|*.cs|All files|*.*";
            // 
            // CodeOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 470);
            this.Controls.Add(this.richEditSourceCode);
            this.Controls.Add(this.toolStrip1);
            this.Name = "CodeOutput";
            this.Text = "Code Output";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripSave;
		private FastRichEdit richEditSourceCode;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
	}
}