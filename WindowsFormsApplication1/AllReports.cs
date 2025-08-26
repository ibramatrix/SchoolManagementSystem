using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class AllReports : Form
    {
        private string firebaseBaseUrl = "YOUR_API_KEY_HERE";
        private Dictionary<string, Dictionary<string, TestReport>> allReportsData;

        private SiticoneComboBox classComboBox;
        private SiticoneComboBox typeComboBox;
        private SiticoneComboBox subjectComboBox;
        private SiticoneDataGridView reportsGridView;

        private SiticoneButton loadReportsBtn;
        private Label headerLabel;

        public AllReports()
        {
            InitializeComponent();
            InitializeUI();
            LoadClasses();
            LoadReportTypes();
            LoadSubjects();
        }

        private void InitializeUI()
        {
            this.Text = "All Test Reports";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            headerLabel = new Label()
            {
                Text = "View Test Reports",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70
            };
            this.Controls.Add(headerLabel);

            int leftMargin = 30, topMargin = 90, spacing = 280, comboHeight = 38, comboWidth = 210;

            // Class Label + ComboBox
            var classLabel = new Label()
            {
                Text = "Class:",
                Location = new Point(leftMargin, topMargin + 7),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(classLabel);

            classComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMargin + 65, topMargin),
                Size = new Size(comboWidth, comboHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(classComboBox);

            // Report Type Label + ComboBox
            var typeLabel = new Label()
            {
                Text = "Type:",
                Location = new Point(leftMargin + spacing, topMargin + 7),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(typeLabel);

            typeComboBox = new SiticoneComboBox()
            {
                Location = new Point(typeLabel.Location.X + 55, topMargin),
                Size = new Size(comboWidth, comboHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(typeComboBox);

            // Subject Label + ComboBox
            var subjectLabel = new Label()
            {
                Text = "Subject:",
                Location = new Point(leftMargin + spacing * 2, topMargin + 7),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(subjectLabel);

            subjectComboBox = new SiticoneComboBox()
            {
                Location = new Point(subjectLabel.Location.X + 75, topMargin),
                Size = new Size(comboWidth, comboHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(subjectComboBox);

            // Load Reports Button
            loadReportsBtn = new SiticoneButton()
            {
                Text = "Load Reports",
                Location = new Point(leftMargin + spacing * 3 + 30, topMargin + 2),
                Size = new Size(140, 40),
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BorderRadius = 10,
                Cursor = Cursors.Hand
            };
            loadReportsBtn.Click += LoadReportsBtn_Click;
            this.Controls.Add(loadReportsBtn);

            // DataGridView for Reports
            reportsGridView = new SiticoneDataGridView()
            {
                Location = new Point(leftMargin, topMargin + 70),
                Size = new Size(this.ClientSize.Width - 2 * leftMargin, this.ClientSize.Height - (topMargin + 100)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Gainsboro,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            reportsGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            reportsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            reportsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            reportsGridView.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            reportsGridView.CellContentClick += ReportsGridView_CellContentClick;

            this.Controls.Add(reportsGridView);
        }

        private void LoadClasses()
        {
            classComboBox.Items.Clear();
            classComboBox.Items.AddRange(new string[] { "P_G", "K_G", "ONE", "TWO", "THREE", "FOUR", "EIGHT" });
            if (classComboBox.Items.Count > 0)
                classComboBox.SelectedIndex = 0;
        }

        private void LoadReportTypes()
        {
            typeComboBox.Items.Clear();
            typeComboBox.Items.Add("All");
            typeComboBox.Items.AddRange(new string[]
            {
                "Weekly", "Monthly", "Mid Term", "Final Term", "Assignment", "Other"
            });
            typeComboBox.SelectedIndex = 0;
        }

        private void LoadSubjects()
        {
            subjectComboBox.Items.Clear();
            subjectComboBox.Items.Add("All");
            subjectComboBox.Items.AddRange(new string[]
            {
                "Mathematics", "Science", "English", "History", "Geography", "Physics", "Chemistry", "Biology"
            });
            subjectComboBox.SelectedIndex = 0;
        }

        private async void LoadReportsBtn_Click(object sender, EventArgs e)
        {
            string selectedClass = classComboBox.SelectedItem?.ToString();
            string selectedType = typeComboBox.SelectedItem?.ToString();
            string selectedSubject = subjectComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Please select a class.");
                return;
            }

            reportsGridView.DataSource = null;
            reportsGridView.Columns.Clear();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string classUrl = $"{firebaseBaseUrl}/TestReports/{selectedClass}.json";
                    string json = await client.GetStringAsync(classUrl);

                    if (string.IsNullOrEmpty(json) || json == "null")
                    {
                        MessageBox.Show("No reports found for selected class.");
                        return;
                    }

                    allReportsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TestReport>>>(json);

                    if (allReportsData == null || allReportsData.Count == 0)
                    {
                        MessageBox.Show("No reports found.");
                        return;
                    }

                    var allReportsList = new List<ReportDisplayItem>();

                    foreach (var monthEntry in allReportsData)
                    {
                        string month = monthEntry.Key;
                        var tests = monthEntry.Value;

                        foreach (var testEntry in tests)
                        {
                            var report = testEntry.Value;

                            bool matchesType = selectedType == "All" || report.ReportType == selectedType;
                            bool matchesSubject = selectedSubject == "All" || report.Subject == selectedSubject;

                            if (matchesType && matchesSubject)
                            {
                                allReportsList.Add(new ReportDisplayItem
                                {
                                    Month = month,
                                    TestKey = testEntry.Key,
                                    TestName = report.TestName,
                                    ReportType = report.ReportType,
                                    Subject = report.Subject,
                                    TotalMarks = report.TotalMarks,
                                    StudentCount = report.Students?.Count ?? 0,
                                    Students = report.Students
                                });
                            }
                        }
                    }

                    if (allReportsList.Count == 0)
                    {
                        MessageBox.Show("No reports matched the selected filters.");
                        return;
                    }

                    reportsGridView.DataSource = allReportsList;

                    // Add SHOW button column
                    var btnColumn = new DataGridViewButtonColumn
                    {
                        HeaderText = "Action",
                        Text = "SHOW",
                        UseColumnTextForButtonValue = true,
                        Name = "ShowButton",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                        Width = 80,
                    };
                    reportsGridView.Columns.Add(btnColumn);

                    // Customize headers
                    reportsGridView.Columns["TestKey"].HeaderText = "Test No";
                    reportsGridView.Columns["TestName"].HeaderText = "Test Name";
                    reportsGridView.Columns["ReportType"].HeaderText = "Type";
                    reportsGridView.Columns["Subject"].HeaderText = "Subject";
                    reportsGridView.Columns["TotalMarks"].HeaderText = "Total Marks";
                    reportsGridView.Columns["StudentCount"].HeaderText = "Students";
                    reportsGridView.Columns["Month"].HeaderText = "Month";

                    // Adjust some column widths for readability
                    reportsGridView.Columns["Month"].Width = 80;
                    reportsGridView.Columns["TestKey"].Width = 80;
                    reportsGridView.Columns["StudentCount"].Width = 80;
                    reportsGridView.Columns["TotalMarks"].Width = 90;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reports: " + ex.Message);
            }
        }

        private void ReportsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (reportsGridView.Columns[e.ColumnIndex].Name == "ShowButton")
            {
                var reportItem = (ReportDisplayItem)reportsGridView.Rows[e.RowIndex].DataBoundItem;
                if (reportItem != null)
                {
                    ShowStudentsReport(reportItem);
                }
            }
        }

        private void ShowStudentsReport(ReportDisplayItem report)
        {
            var studentsForm = new Form()
            {
                Text = $"Students Marks: {report.TestName} ({report.Month})",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            var dgv = new SiticoneDataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 35 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Gainsboro,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            // Columns: StudentID, Name, ObtainedMarks
            dgv.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Student ID", DataPropertyName = "StudentID" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Name", DataPropertyName = "Name" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Obtained Marks", DataPropertyName = "ObtainedMarks" });

            // Prepare data source
            var studentList = report.Students.Select(s => new
            {
                StudentID = s.Key,
                Name = s.Value.Name,
                ObtainedMarks = s.Value.ObtainedMarks
            }).ToList();

            dgv.DataSource = studentList;

            studentsForm.Controls.Add(dgv);
            studentsForm.ShowDialog();
        }

        // Helper class for display in DataGridView
        private class ReportDisplayItem
        {
            public string Month { get; set; }
            public string TestKey { get; set; }
            public string TestName { get; set; }
            public string ReportType { get; set; }
            public string Subject { get; set; }
            public int TotalMarks { get; set; }
            public int StudentCount { get; set; }

            // Keep the dictionary so we can show students later
            public Dictionary<string, TestStudentRecord> Students { get; set; }
        }
    }
}
