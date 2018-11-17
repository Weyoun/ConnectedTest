namespace WindowsFormsApp2
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.GraphView = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // GraphView
            // 
            this.GraphView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphView.Location = new System.Drawing.Point(0, 0);
            this.GraphView.Name = "GraphView";
            this.GraphView.Size = new System.Drawing.Size(800, 450);
            this.GraphView.TabIndex = 0;
            this.GraphView.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphView_Paint);
            this.GraphView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphView_MouseDown);
            this.GraphView.Resize += new System.EventHandler(this.GraphView_Resize);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.GraphView);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel GraphView;
    }
}

