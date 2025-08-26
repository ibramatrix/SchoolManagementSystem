using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class EditStudentForm : Form
    {
        public Student UpdatedStudent { get; private set; }
        private Student originalStudent;

        // UI Controls
        private Label lblName, lblFatherName, lblAddress, lblContact, lblFee;
        private TextBox txtName, txtFatherName, txtAddress, txtContact, txtFee;
        private Button btnSave;

        public EditStudentForm(Student student)
        {
            originalStudent = student;

            this.Text = "Edit Student";
            this.Size = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font textboxFont = new Font("Segoe UI", 10);

            // Name
            lblName = new Label() { Text = "Name", Location = new Point(30, 30), Size = new Size(120, 25), Font = labelFont };
            txtName = new TextBox() { Location = new Point(160, 30), Size = new Size(180, 25), Font = textboxFont };

            // Father Name
            lblFatherName = new Label() { Text = "Father Name", Location = new Point(30, 70), Size = new Size(120, 25), Font = labelFont };
            txtFatherName = new TextBox() { Location = new Point(160, 70), Size = new Size(180, 25), Font = textboxFont };

            // Address
            lblAddress = new Label() { Text = "Address", Location = new Point(30, 110), Size = new Size(120, 25), Font = labelFont };
            txtAddress = new TextBox() { Location = new Point(160, 110), Size = new Size(180, 25), Font = textboxFont };

            // Contact
            lblContact = new Label() { Text = "Contact", Location = new Point(30, 150), Size = new Size(120, 25), Font = labelFont };
            txtContact = new TextBox() { Location = new Point(160, 150), Size = new Size(180, 25), Font = textboxFont };

            // Fee
            lblFee = new Label() { Text = "Fee", Location = new Point(30, 190), Size = new Size(120, 25), Font = labelFont };
            txtFee = new TextBox() { Location = new Point(160, 190), Size = new Size(180, 25), Font = textboxFont };

            // Save Button
            btnSave = new Button()
            {
                Text = "💾 Save",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(130, 250)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;

            // Add controls to form
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblFatherName);
            Controls.Add(txtFatherName);
            Controls.Add(lblAddress);
            Controls.Add(txtAddress);
            Controls.Add(lblContact);
            Controls.Add(txtContact);
            Controls.Add(lblFee);
            Controls.Add(txtFee);
            Controls.Add(btnSave);

            // Fill values
            txtName.Text = student.Name;
            txtFatherName.Text = student.FatherName;
            txtAddress.Text = student.Address;
            txtContact.Text = student.Contact;
            txtFee.Text = student.Fee;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            UpdatedStudent = new Student
            {
                Name = txtName.Text.Trim(),
                FatherName = txtFatherName.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Contact = txtContact.Text.Trim(),
                Fee = txtFee.Text.Trim(),
                FeeStatus = originalStudent.FeeStatus // preserve original fee status
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
