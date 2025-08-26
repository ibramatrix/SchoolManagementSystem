using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    partial class FingerprintEnroller
    {
        private System.ComponentModel.IContainer components = null;
        private Label titleLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(628, 19);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(231, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "🔐 Enroll Fingerprint";
            // 
           
            // 
            // FingerprintEnroller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.titleLabel);
            this.Name = "FingerprintEnroller";
            this.Size = new System.Drawing.Size(895, 604);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
