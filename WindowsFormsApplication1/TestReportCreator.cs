using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Agreement.JPake;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class TestReportCreator : Form
    {
        private string studentsFilePath = "Students.json";

        private BindingList<StudentRow> studentRows;
        private Dictionary<string, Dictionary<string, Student>> allClasses;

        // UI Controls
        private SiticoneComboBox classComboBox;
        private SiticoneComboBox typeComboBox;
        private SiticoneComboBox subjectComboBox;

        private SiticoneTextBox testNameTextBox;
        private SiticoneTextBox totalMarksTextBox;

        private SiticoneButton createReportBtn;
        private SiticoneButton saveReportBtn;

        private SiticoneDataGridView studentsGridView;

        private Label headerLabel;
        private Label reportCountLabel;

        public TestReportCreator()
        {
            InitializeComponent();
            InitializeUI();
            LoadClasses();
            LoadReportTypes();
            LoadSubjects();  // Can be replaced with dynamic loading
        }

        private void InitializeUI()
        {
            this.Text = "Test Report Creator";
            this.Size = new Size(1000, 800);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Header Label
            headerLabel = new Label()
            {
                Text = "Create Test Report",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70,
            };
            this.Controls.Add(headerLabel);

            int topMargin = 90;
            int controlSpacing = 20;
            int labelWidth = 120;
            int comboBoxWidth = 180;
            int textBoxWidth = 200;
            int leftMargin = 50;

            // Class ComboBox
            var classLabel = new Label()
            {
                Text = "Select Class:",
                Location = new Point(leftMargin, topMargin),
                Size = new Size(labelWidth, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(classLabel);

            classComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin),
                Size = new Size(comboBoxWidth, 36),
                FillColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 8,
                ShadowDecoration = { Enabled = true }
            };
            classComboBox.SelectedIndexChanged += ClassComboBox_SelectedIndexChanged;
            this.Controls.Add(classComboBox);

            // Report Type ComboBox
            var typeLabel = new Label()
            {
                Text = "Report Type:",
                Location = new Point(leftMargin + labelWidth + comboBoxWidth + controlSpacing, topMargin),
                Size = new Size(labelWidth, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(typeLabel);

            typeComboBox = new SiticoneComboBox()
            {
                Location = new Point(typeLabel.Location.X + labelWidth, topMargin),
                Size = new Size(comboBoxWidth, 36),
                FillColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 8,
                ShadowDecoration = { Enabled = true }
            };
            this.Controls.Add(typeComboBox);

            // Subject ComboBox
            var subjectLabel = new Label()
            {
                Text = "Select Subject:",
                Location = new Point(typeComboBox.Location.X + comboBoxWidth + controlSpacing, topMargin),
                Size = new Size(labelWidth, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(subjectLabel);

            subjectComboBox = new SiticoneComboBox()
            {
                Location = new Point(subjectLabel.Location.X + labelWidth, topMargin),
                Size = new Size(comboBoxWidth, 36),
                FillColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 8,
                ShadowDecoration = { Enabled = true }
            };
            this.Controls.Add(subjectComboBox);

            // Test Name TextBox
            var testNameLabel = new Label()
            {
                Text = "Test Name:",
                Location = new Point(leftMargin, topMargin + 60),
                Size = new Size(labelWidth, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(testNameLabel);

            testNameTextBox = new SiticoneTextBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin + 60),
                Size = new Size(textBoxWidth, 36),
                PlaceholderText = "Enter test name"
            };
            this.Controls.Add(testNameTextBox);

            // Total Marks TextBox
            var totalMarksLabel = new Label()
            {
                Text = "Total Marks:",
                Location = new Point(leftMargin + labelWidth + textBoxWidth + controlSpacing, topMargin + 60),
                Size = new Size(labelWidth, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(totalMarksLabel);

            totalMarksTextBox = new SiticoneTextBox()
            {
                Location = new Point(totalMarksLabel.Location.X + labelWidth, topMargin + 60),
                Size = new Size(100, 36),
                PlaceholderText = "Max marks"
            };
            this.Controls.Add(totalMarksTextBox);

            // Buttons
            createReportBtn = new SiticoneButton()
            {
                Text = "Create Report",
                Location = new Point(totalMarksTextBox.Location.X + 120, topMargin + 60),
                Size = new Size(130, 40),
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8
            };
            createReportBtn.Click += CreateReportBtn_Click;
            this.Controls.Add(createReportBtn);

            saveReportBtn = new SiticoneButton()
            {
                Text = "Save Report",
                Location = new Point(createReportBtn.Location.X + 150, topMargin + 60),
                Size = new Size(130, 40),
                FillColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8,
                Visible = false
            };
            saveReportBtn.Click += SaveReportBtn_Click;
            this.Controls.Add(saveReportBtn);

            // Report Count Label
            reportCountLabel = new Label()
            {
                Text = "Test Reports Created: 0",
                Location = new Point(leftMargin, topMargin + 110),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(reportCountLabel);

            // Students DataGridView
            studentsGridView = new SiticoneDataGridView()
            {
                Location = new Point(leftMargin, topMargin + 150),
                Size = new Size(900, 500),
                ReadOnly = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Gainsboro,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            // Style headers
            studentsGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            studentsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            studentsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            studentsGridView.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            this.Controls.Add(studentsGridView);

            this.Resize += (s, e) =>
            {
                studentsGridView.Location = new Point(leftMargin, this.ClientSize.Height - 550);
            };
        }

        private void LoadClasses()
        {
            if (!File.Exists(studentsFilePath))
            {
                MessageBox.Show("Students.json file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(studentsFilePath);

            try
            {
                allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);

                classComboBox.Items.Clear();
                foreach (var className in allClasses.Keys)
                {
                    classComboBox.Items.Add(className);
                }

                if (classComboBox.Items.Count > 0)
                    classComboBox.SelectedIndex = 0; // Select first class by default
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load classes: " + ex.Message);
            }
        }

        private void LoadReportTypes()
        {
            typeComboBox.Items.Clear();
            typeComboBox.Items.AddRange(new string[]
            {
                "Weekly",
                "Monthly",
                "Mid Term",
                "Final Term",
                "Assignment",
                "Other"
            });
            typeComboBox.SelectedIndex = 0;
        }

        private void LoadSubjects()
        {
            // You can replace this with dynamic subjects loading logic or a separate file
            subjectComboBox.Items.Clear();
            subjectComboBox.Items.AddRange(new string[]
            {
                "Mathematics",
                "Science",
                "English",
                "History",
                "Geography",
                "Physics",
                "Chemistry",
                "Biology"
            });
            subjectComboBox.SelectedIndex = 0;
        }

        private void ClassComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear any existing data when class changes
            studentsGridView.DataSource = null;
            createReportBtn.Visible = true;
            saveReportBtn.Visible = false;
        }

        private void CreateReportBtn_Click(object sender, EventArgs e)
        {
            string selectedClass = classComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Please select a class.");
                return;
            }

            if (!allClasses.ContainsKey(selectedClass))
            {
                MessageBox.Show("Class data not found.");
                return;
            }

            var studentsDict = allClasses[selectedClass];

            studentRows = new BindingList<StudentRow>(
                studentsDict.Select(s => new StudentRow
                {
                    StudentID = s.Key,
                    Name = s.Value.Name,
                    FatherName = s.Value.FatherName,
                    Address = s.Value.Address,
                    ObtainedMarks = 0
                }).ToList()
            );

            studentsGridView.DataSource = studentRows;

            // Allow editing only ObtainedMarks column
            foreach (DataGridViewColumn col in studentsGridView.Columns)
            {
                col.ReadOnly = col.Name != "ObtainedMarks";
            }

            createReportBtn.Visible = false;
            saveReportBtn.Visible = true;
        }

        private async void SaveReportBtn_Click(object sender, EventArgs e)
        {
            if (studentRows == null || studentRows.Count == 0)
            {
                MessageBox.Show("No data to save.");
                return;
            }

            string selectedClass = classComboBox.SelectedItem?.ToString();
            string reportType = typeComboBox.SelectedItem?.ToString();
            string subject = subjectComboBox.SelectedItem?.ToString();
            string testName = testNameTextBox.Text.Trim();
            string totalMarksText = totalMarksTextBox.Text.Trim();

            if (string.IsNullOrEmpty(testName))
            {
                MessageBox.Show("Please enter the test name.");
                return;
            }

            if (!int.TryParse(totalMarksText, out int totalMarks) || totalMarks <= 0)
            {
                MessageBox.Show("Please enter a valid total marks.");
                return;
            }

            string monthName = DateTime.Now.ToString("MMMM");

            var studentsData = studentRows.ToDictionary(
                s => s.StudentID,
                s => new TestStudentRecord
                {
                    Name = s.Name,
                    ObtainedMarks = s.ObtainedMarks
                });

            var report = new TestReport
            {
                TestName = testName,
                TotalMarks = totalMarks,
                ReportType = reportType,
                Subject = subject,
                Students = studentsData
            };

            string baseUrl = "YOUR_API_KEY_HERE";
            string reportPath = $"/TestReports/{selectedClass}/{monthName}.json";
            string url = baseUrl + reportPath;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string existingJson = await client.GetStringAsync(url);
                    var existingReports = JsonConvert.DeserializeObject<Dictionary<string, TestReport>>(existingJson)
                                          ?? new Dictionary<string, TestReport>();

                    string testKey = $"Test{(existingReports.Count + 1):000}";
                    existingReports[testKey] = report;

                    string finalJson = JsonConvert.SerializeObject(existingReports, Formatting.Indented);
                    var content = new StringContent(finalJson, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("❌ Failed to save test report to Firebase.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Success UI
                    MessageBox.Show("✅ Test report saved to Firebase!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    reportCountLabel.Text = $"Test Reports Created: {existingReports.Count}";
                    saveReportBtn.Visible = false;
                    createReportBtn.Visible = true;
                    studentsGridView.DataSource = null;
                    testNameTextBox.Clear();
                    totalMarksTextBox.Clear();

                    // --- Send notifications safely ---
                    string studentsUrl = $"{baseUrl}/students/{selectedClass}.json";
                    string studentsJson = await client.GetStringAsync(studentsUrl);
                    var studentsDict = JsonConvert.DeserializeObject<Dictionary<string, Student>>(studentsJson);

                    if (studentsDict != null)
                    {
                        foreach (var student in studentsDict.Values)
                        {
                            if (!string.IsNullOrEmpty(student.fcmToken))
                            {
                                Console.WriteLine($"🔹 Preparing to send notification to {student.Name} ({student.fcmToken})...");

                                try
                                {
                                    await FcmSender.SendNotificationAsync(
                                        deviceToken: student.fcmToken,
                                        title: "New Test Report",
                                        body: $"A new test report '{testName}' has been added for your class!"
                                    );

                                    Console.WriteLine($"✅ Notification successfully sent to {student.Name}!");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"⚠️ Failed to send notification to {student.Name}: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Skipping {student.Name} because DeviceToken is empty or null.");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("🔥 Error uploading test report: " + ex.Message);
            }
        }


    }

    // Data models (you can keep your existing ones or add extra properties)






}
