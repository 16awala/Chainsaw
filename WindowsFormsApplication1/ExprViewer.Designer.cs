namespace WindowsFormsApplication1
{
    partial class ExprViewer
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.sliderpanel = new System.Windows.Forms.Panel();
            this.ExpressionText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(336, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(806, 626);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // sliderpanel
            // 
            this.sliderpanel.Location = new System.Drawing.Point(12, 12);
            this.sliderpanel.Name = "sliderpanel";
            this.sliderpanel.Size = new System.Drawing.Size(318, 626);
            this.sliderpanel.TabIndex = 1;
            // 
            // ExpressionText
            // 
            this.ExpressionText.AutoSize = true;
            this.ExpressionText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExpressionText.Location = new System.Drawing.Point(336, 12);
            this.ExpressionText.Name = "ExpressionText";
            this.ExpressionText.Size = new System.Drawing.Size(21, 31);
            this.ExpressionText.TabIndex = 0;
            this.ExpressionText.Text = " ";
            // 
            // ExprViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 650);
            this.Controls.Add(this.ExpressionText);
            this.Controls.Add(this.sliderpanel);
            this.Controls.Add(this.pictureBox1);
            this.Name = "ExprViewer";
            this.Text = "Expression Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel sliderpanel;
        private System.Windows.Forms.Label ExpressionText;
    }
}

