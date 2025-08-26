using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class NewStudentForm : Form
    {
        private TextBox nameTxt, fatherNameTxt, addressTxt, contactTxt, feeTxt;
        private ComboBox classComboBox;
        private Button saveBtn;

        private Dictionary<string, Dictionary<string, Student>> allClasses;
        private string filePath = "Students.json";

        public NewStudentForm()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadClassDataAsync(); // load classes asynchronously after UI is ready
        }

        private async Task LoadClassDataAsync()
        {
            try
            {
                // Fetch all students grouped by class from Firebase
                allClasses = await FirebaseHelper.LoadClassData();

                // Populate the class dropdown
                classComboBox.Items.Clear();
                foreach (var className in allClasses.Keys)
                {
                    classComboBox.Items.Add(className);
                }

                // Select first class by default if available
                if (classComboBox.Items.Count > 0)
                {
                    classComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching classes from Firebase: " + ex.Message);
                allClasses = new Dictionary<string, Dictionary<string, Student>>();
            }
        }

        private void InitializeUI()
        {
            this.Text = "➕ Add New Student";
            this.Size = new Size(520, 580);
            this.BackColor = Color.White;

            Label titleLabel = new Label()
            {
                Text = "New Student Entry",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
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
            fatherNameTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(fatherNameTxt);

            AddLabel("Address:", top += 40);
            addressTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(addressTxt);

            AddLabel("Contact No:", top += 40);
            contactTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(contactTxt);

            AddLabel("Fee:", top += 40);
            feeTxt = new TextBox() { Location = new Point(170, top), Width = inputWidth };
            this.Controls.Add(feeTxt);

            AddLabel("Class:", top += 40);
            classComboBox = new ComboBox()
            {
                Location = new Point(170, top),
                Width = inputWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(classComboBox);

            saveBtn = new Button()
            {
                Text = "💾 Save Student",
                Location = new Point(170, top + 60),
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
            string father = fatherNameTxt.Text.Trim();
            string address = addressTxt.Text.Trim();
            string contact = contactTxt.Text.Trim();
            string fee = feeTxt.Text.Trim();
            string selectedClass = classComboBox.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(father) ||
                string.IsNullOrWhiteSpace(contact) || string.IsNullOrWhiteSpace(fee) ||
                string.IsNullOrWhiteSpace(selectedClass))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (!allClasses.ContainsKey(selectedClass))
                allClasses[selectedClass] = new Dictionary<string, Student>();

            string newId = GenerateNewStudentId(allClasses[selectedClass]);

            Student newStudent = new Student
            {
                Name = name,
                FatherName = father,
                Address = address,
                Contact = contact,
                Fee = fee,
                AdmissionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            allClasses[selectedClass][newId] = newStudent;

            string updatedJson = JsonConvert.SerializeObject(allClasses, Formatting.Indented);
            File.WriteAllText(filePath, updatedJson);

            await UploadStudentToFirebase(selectedClass, newId, newStudent);

            MessageBox.Show($"✅ Student added with ID: {newId} and uploaded to Firebase.");
            this.Close();
        }

        private string GenerateNewStudentId(Dictionary<string, Student> classStudents)
        {
            int maxId = classStudents.Keys
                .Where(k => k.StartsWith("Student"))
                .Select(k => int.TryParse(k.Replace("Student", ""), out int n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"Student{(maxId + 1):D3}";
        }

        private async Task UploadStudentToFirebase(string className, string studentId, Student student)
        {
            using (var client = new HttpClient())
            {
                string firebaseUrl = $"YOUR_API_KEY_HERE{className}/{studentId}.json";
                var json = JsonConvert.SerializeObject(student);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(firebaseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("❌ Failed to upload student to Firebase.");
                }
            }
        }
    }
}
