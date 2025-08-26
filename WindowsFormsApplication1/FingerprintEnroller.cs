using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class FingerprintEnroller : UserControl
    {
        private Dictionary<string, Dictionary<string, Student>> allClasses;

        private SiticoneComboBox classComboBox;
        private SiticoneComboBox studentComboBox;
        private SiticoneComboBox staffComboBox;
        private SiticoneButton enrollButton;
        private SiticoneTextBox logBox;

        private bool isStaffMode = false;
        private static readonly string firebaseHost = "YOUR_API_KEY_HERE";

        public FingerprintEnroller()
        {
            InitializeComponent();
            InitializeUI();
            AskUserType();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Title Label
            var titleLabel = new SiticoneHtmlLabel()
            {
                Text = "📥 Fingerprint Enroller",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Class ComboBox
            classComboBox = new SiticoneComboBox()
            {
                Location = new Point(20, 60),
                Width = 180,
                BorderColor = Color.FromArgb(41, 128, 185),
                FillColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            classComboBox.SelectedIndexChanged += ClassComboBox_SelectedIndexChanged;
            this.Controls.Add(classComboBox);

            // Student ComboBox
            studentComboBox = new SiticoneComboBox()
            {
                Location = new Point(220, 60),
                Width = 250,
                BorderColor = Color.FromArgb(41, 128, 185),
                FillColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(studentComboBox);

            // Staff ComboBox (hidden initially)
            staffComboBox = new SiticoneComboBox()
            {
                Location = new Point(20, 60),
                Width = 250,
                BorderColor = Color.FromArgb(41, 128, 185),
                FillColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            this.Controls.Add(staffComboBox);

            // Enroll Button
            enrollButton = new SiticoneButton()
            {
                Text = "📤 Enroll Fingerprint",
                Location = new Point(500, 60),
                Size = new Size(180, 40),
                BorderRadius = 8,
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                HoverState = { FillColor = Color.FromArgb(52, 152, 219) }
            };
            enrollButton.Click += EnrollFingerprintBtn_Click;
            this.Controls.Add(enrollButton);

            // Log box
            logBox = new SiticoneTextBox()
            {
                Location = new Point(20, 120),
                Size = new Size(650, 250),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderColor = Color.FromArgb(41, 128, 185),
                FillColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Consolas", 10),
                BorderRadius = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(logBox);

            this.Resize += (s, e) =>
            {
                logBox.Width = this.Width - 40;
                logBox.Height = this.Height - 140;
            };
        }

        private void AskUserType()
        {
            var result = MessageBox.Show("Do you want to enroll a Staff fingerprint?", "Select Type", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                isStaffMode = true;
                classComboBox.Visible = false;
                studentComboBox.Visible = false;
                staffComboBox.Visible = true;

                LoadStaffComboBox();
            }
            else
            {
                isStaffMode = false;
                classComboBox.Visible = true;
                studentComboBox.Visible = true;
                staffComboBox.Visible = false;

                LoadStudentsFromJson();
            }
        }

        private void LoadStaffComboBox()
        {
            string staffFile = "Staff.json";

            if (!File.Exists(staffFile))
            {
                MessageBox.Show("❌ Staff.json not found.");
                return;
            }

            string json = File.ReadAllText(staffFile);
            var staffList = JsonConvert.DeserializeObject<Dictionary<string, Staff>>(json);

            staffComboBox.Items.Clear();
            foreach (var s in staffList)
            {
                staffComboBox.Items.Add($"{s.Key} - {s.Value.Name}");
            }

            if (staffComboBox.Items.Count > 0)
                staffComboBox.SelectedIndex = 0;
        }

        private void LoadStudentsFromJson()
        {
            string path = "Students.json";
            if (!File.Exists(path))
            {
                MessageBox.Show("❌ Students.json not found.");
                return;
            }

            string json = File.ReadAllText(path);
            allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);

            classComboBox.Items.Clear();
            foreach (var className in allClasses.Keys)
            {
                classComboBox.Items.Add(className);
            }

            if (classComboBox.Items.Count > 0)
                classComboBox.SelectedIndex = 0;
        }

        private void ClassComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            studentComboBox.Items.Clear();

            string selectedClass = classComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClass)) return;

            if (allClasses.ContainsKey(selectedClass))
            {
                foreach (var student in allClasses[selectedClass])
                {
                    studentComboBox.Items.Add($"{student.Key} - {student.Value.Name}");
                }
            }

            if (studentComboBox.Items.Count > 0)
                studentComboBox.SelectedIndex = 0;
        }

        private async void EnrollFingerprintBtn_Click(object sender, EventArgs e)
        {
            string command = "";

            if (isStaffMode)
            {
                if (staffComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select staff.");
                    return;
                }

                string entry = staffComboBox.SelectedItem.ToString();
                string idText = entry.Split('-')[0].Trim().Replace("Staff", "");

                if (!int.TryParse(idText, out int id))
                {
                    MessageBox.Show("Invalid staff ID.");
                    return;
                }

                command = $"ENROLL,Staff,{id}";
            }
            else
            {
                if (classComboBox.SelectedIndex == -1 || studentComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select class and student.");
                    return;
                }

                string className = classComboBox.SelectedItem.ToString();
                string studentEntry = studentComboBox.SelectedItem.ToString();
                string studentId = studentEntry.Split('-')[0].Trim();
                string numericId = studentId.Replace("Student", "");

                if (!int.TryParse(numericId, out int sid))
                {
                    MessageBox.Show("Invalid student ID.");
                    return;
                }

                command = $"ENROLL,{className},{sid}";
            }

            bool success = await SendCommandToFirebase(command);

            if (success)
                logBox.AppendText($"✅ Sent: {command}\n");
            else
                logBox.AppendText($"❌ Failed to send: {command}\n");

            logBox.ScrollToCaret();
        }

        private async Task<bool> SendCommandToFirebase(string command)
        {
            string url = $"{firebaseHost}/Commands/sensorA.json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var payload = new
                    {
                        command = command,
                        status = "pending"
                    };

                    string json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync(url, content);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    logBox.AppendText($"❌ Error: {ex.Message}\n");
                    return false;
                }
            }
        }

    }
}
