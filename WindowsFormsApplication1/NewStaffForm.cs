using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApplication1
{
    public partial class NewStaffForm : Form
    {
        private TextBox nameTxt, fatherTxt, salaryTxt, addressTxt;
        private Button saveBtn;
        private ComboBox staffIDComboBox;

        public NewStaffForm()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadAvailableStaffIDsAsync();
        }

        private void InitializeUI()
        {
            this.Text = "➕ Add New Staff Member";
            this.Size = new Size(520, 500);
            this.BackColor = Color.White;

            Label titleLabel = new Label()
            {
                Text = "New Staff Entry",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Size = new Size(this.Width, 60),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            int top = 100;
            int labelWidth = 120;
            int inputWidth = 300;

            Label AddLabel(string text, int y)
            {
                Label lbl = new Label()
                {
                    Text = text,
                    Location = new Point(40, y),
                    Size = new Size(labelWidth, 30),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                this.Controls.Add(lbl);
                return lbl;
            }

            AddLabel("Name:", top);
            nameTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(nameTxt);

            AddLabel("Father Name:", top += 40);
            fatherTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(fatherTxt);

            AddLabel("Salary:", top += 40);
            salaryTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(salaryTxt);

            AddLabel("Address:", top += 40);
            addressTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth, Height = 60, Multiline = true };
            this.Controls.Add(addressTxt);

            AddLabel("Staff ID:", top += 70);
            staffIDComboBox = new ComboBox()
            {
                Location = new Point(170, top),
                Width = inputWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            this.Controls.Add(staffIDComboBox);

            saveBtn = new Button()
            {
                Text = "💾 Save Staff",
                Location = new Point(170, top + 50),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            saveBtn.Click += SaveBtn_Click;
            this.Controls.Add(saveBtn);
        }

        private async void SaveBtn_Click(object sender, EventArgs e)
        {
            string name = nameTxt.Text.Trim();
            string father = fatherTxt.Text.Trim();
            string salary = salaryTxt.Text.Trim();
            string address = addressTxt.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(father) ||
                string.IsNullOrWhiteSpace(salary) || string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            string selectedStaffId = staffIDComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedStaffId))
            {
                MessageBox.Show("Please select a Staff ID.");
                return;
            }

            Staff newStaff = new Staff
            {
                Name = name,
                FatherName = father,
                Phone = "",
                Salary = salary,
                Address = address,
                Username = null,
                Password = null,
                Manages = null,
                isActive = false
            };

            try
            {
                await UploadStaffToFirebaseAsync(selectedStaffId, newStaff);
                MessageBox.Show($"✅ Staff added with ID: {selectedStaffId} to Firebase.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Failed to upload to Firebase:\n{ex.Message}");
            }
        }

        private async Task UploadStaffToFirebaseAsync(string staffId, Staff staff)
        {
            string url = $"YOUR_API_KEY_HERE{staffId}.json";
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(staff);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Firebase upload failed.");
            }
        }

        private async Task LoadAvailableStaffIDsAsync()
        {
            try
            {
                string firebaseUrl = "YOUR_API_KEY_HERE";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(firebaseUrl);
                    var existingStaff = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();

                    List<string> freeIds = new List<string>();
                    for (int i = 1; i <= 40; i++)
                    {
                        string id = $"Staff{i}";
                        if (!existingStaff.ContainsKey(id))
                            freeIds.Add(id);
                    }

                    staffIDComboBox.Items.Clear();
                    staffIDComboBox.Items.AddRange(freeIds.ToArray());
                    if (freeIds.Count > 0) staffIDComboBox.SelectedIndex = 0;
                    else MessageBox.Show("⚠ No free Staff IDs available!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("🔥 Error loading staff IDs:\n" + ex.Message);
            }
        }
    }
}
