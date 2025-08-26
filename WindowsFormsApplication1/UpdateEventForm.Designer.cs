using System.Drawing;
using System.Windows.Forms;
using System;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    partial class UpdateEventForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private SiticoneTextBox titleTxt;
        private SiticoneTextBox timeTxt;
        private SiticoneDateTimePicker datePicker;
        private SiticoneComboBox statusCombo;
        private SiticoneButton updateBtn;
        private SiticoneButton cancelBtn;
        private SiticoneElipse siticoneElipse1;
        private SiticoneShadowForm siticoneShadowForm1;

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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "UpdateEventForm";

            this.titleTxt = new Siticone.Desktop.UI.WinForms.SiticoneTextBox();
            this.timeTxt = new Siticone.Desktop.UI.WinForms.SiticoneTextBox();
            this.datePicker = new Siticone.Desktop.UI.WinForms.SiticoneDateTimePicker();
            this.statusCombo = new Siticone.Desktop.UI.WinForms.SiticoneComboBox();
            this.updateBtn = new Siticone.Desktop.UI.WinForms.SiticoneButton();
            this.cancelBtn = new Siticone.Desktop.UI.WinForms.SiticoneButton();
            this.siticoneElipse1 = new Siticone.Desktop.UI.WinForms.SiticoneElipse();
            this.siticoneShadowForm1 = new Siticone.Desktop.UI.WinForms.SiticoneShadowForm();
            this.SuspendLayout();

            // titleTxt
            this.titleTxt.PlaceholderText = "Enter title";
            this.titleTxt.Location = new Point(40, 40);
            this.titleTxt.Size = new Size(320, 36);

            // timeTxt
            this.timeTxt.PlaceholderText = "e.g. 2:00 PM";
            this.timeTxt.Location = new Point(40, 90);
            this.timeTxt.Size = new Size(320, 36);

            // datePicker
            this.datePicker.Format = DateTimePickerFormat.Long;
            this.datePicker.Location = new Point(40, 140);
            this.datePicker.Size = new Size(320, 36);

            // statusCombo
            this.statusCombo.Items.AddRange(new object[] { "Upcoming", "Done", "Cancelled" });
            this.statusCombo.Location = new Point(40, 190);
            this.statusCombo.Size = new Size(320, 36);
            this.statusCombo.StartIndex = 0;

            // updateBtn
            this.updateBtn.Text = "Update";
            this.updateBtn.FillColor = Color.MediumSeaGreen;
            this.updateBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.updateBtn.ForeColor = Color.White;
            this.updateBtn.Location = new Point(40, 250);
            this.updateBtn.Size = new Size(150, 40);
            this.updateBtn.Click += new EventHandler(this.updateBtn_Click);

            // cancelBtn
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.FillColor = Color.Crimson;
            this.cancelBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.cancelBtn.ForeColor = Color.White;
            this.cancelBtn.Location = new Point(210, 250);
            this.cancelBtn.Size = new Size(150, 40);
            this.cancelBtn.Click += (s, e) => this.Close();

            // siticoneElipse1
            this.siticoneElipse1.BorderRadius = 20;
            this.siticoneElipse1.TargetControl = this;

            // Form Settings
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 320);
            this.Controls.Add(this.titleTxt);
            this.Controls.Add(this.timeTxt);
            this.Controls.Add(this.datePicker);
            this.Controls.Add(this.statusCombo);
            this.Controls.Add(this.updateBtn);
            this.Controls.Add(this.cancelBtn);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "UpdateEventForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Update Event";
            this.ResumeLayout(false);
        }

      

        #endregion
    }
}