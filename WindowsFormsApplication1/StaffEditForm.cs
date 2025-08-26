using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{


    public partial class StaffEditForm : Form
    {
        private string staffId;
        private Staff staff;

        TextBox txtName, txtFather, txtPhone, txtSalary, txtAddress, txtUsername, txtPassword;
        ComboBox cmbClass;
        CheckBox chkIsActive;

        public StaffEditForm(string staffId, Staff staff)
        {
            this.staffId = staffId;
            this.staff = staff;

            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Edit Staff";
            this.Size = new Size(500, 600);
            this.BackColor = Color.White;

            Label header = new Label
            {
                Text = $"Editing: {staff.Name}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(header);

            int y = 70;
            txtName = CreateTextbox("Name", staff.Name, ref y);
            txtFather = CreateTextbox("Father Name", staff.FatherName, ref y);
            txtPhone = CreateTextbox("Phone", staff.Phone, ref y);
            txtSalary = CreateTextbox("Salary", staff.Salary, ref y);
            txtAddress = CreateTextbox("Address", staff.Address, ref y);
            txtUsername = CreateTextbox("Username", staff.Username ?? "", ref y);
            txtPassword = CreateTextbox("Password", staff.Password ?? "", ref y);

            chkIsActive = new CheckBox
            {
                Text = "Is Active",
                Location = new Point(160, y),
                Font = new Font("Segoe UI", 10),
                Checked = staff.isActive
            };
            this.Controls.Add(chkIsActive);
            y += 40;



            Label lblClass = new Label
            {
                Text = "Class (Manages)",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(lblClass);

            cmbClass = new ComboBox
            {
                Location = new Point(160, y),
                Width = 280,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbClass.Items.AddRange(new string[] { "None", "P_G","K_G", "NURSERY", "ONE", "TWO", "THREE", "FOUR", "FIVE","SIX","SEVEN","EIGHT","NINETH","TENTH","IT_ADMIN","Principal","Director" });
            cmbClass.SelectedItem = string.IsNullOrEmpty(staff.Manages) ? "None" : staff.Manages;
            this.Controls.Add(cmbClass);
            y += 40;

            Button btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, y + 20),
                Width = 420,
                Height = 40,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private TextBox CreateTextbox(string label, string value, ref int y)
        {
            Label lbl = new Label
            {
                Text = label,
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(lbl);

            TextBox txt = new TextBox
            {
                Text = value,
                Location = new Point(160, y),
                Width = 280,
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(txt);
            y += 40;
            return txt;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            staff.Name = txtName.Text;
            staff.FatherName = txtFather.Text;
            staff.Phone = txtPhone.Text;
            staff.Salary = txtSalary.Text;
            staff.Address = txtAddress.Text;
            staff.Username = txtUsername.Text;
            staff.Password = txtPassword.Text;
            staff.Manages = cmbClass.SelectedItem?.ToString() == "None" ? null : cmbClass.SelectedItem?.ToString();
            staff.isActive = chkIsActive.Checked;

            string json = JsonConvert.SerializeObject(staff);
            string firebaseUrl = $"YOUR_API_KEY_HERE{staffId}.json";

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                try
                {
                    // 1. Update Firebase
                    client.UploadString(firebaseUrl, "PUT", json);

                    // 2. Update Offline JSON
                    string localPath = "Staff.json";
                    if (File.Exists(localPath))
                    {
                        string localJson = File.ReadAllText(localPath);
                        var localData = JsonConvert.DeserializeObject<Dictionary<string, Staff>>(localJson);

                        if (localData.ContainsKey(staffId))
                            localData[staffId] = staff;
                        else
                            localData.Add(staffId, staff);

                        File.WriteAllText(localPath, JsonConvert.SerializeObject(localData, Formatting.Indented));
                    }

                    MessageBox.Show("Staff updated successfully!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving to Firebase or local file: " + ex.Message);
                }
            }
        }


    }
}
