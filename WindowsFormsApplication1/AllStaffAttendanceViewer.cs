using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class AllStaffAttendanceViewer : Form
    {
        private SiticoneComboBox monthComboBox;
        private SiticoneComboBox yearComboBox;
        private SiticoneButton syncBtn;
        private SiticoneDataGridView dgv;
        private Label headerLabel;
        private Label summaryLabel;

        private Dictionary<string, StaffInfo> staffDirectory; // Map staffId => staffName

        public AllStaffAttendanceViewer()
        {
            InitializeComponent();
            InitializeUI();
            // Note: staffDirectory is loaded inside LoadAttendanceAsync to ensure latest data
        }

        private void InitializeUI()
        {
            this.Text = "All Staff Attendance";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            headerLabel = new Label()
            {
                Text = "All Staff Attendance",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70
            };
            this.Controls.Add(headerLabel);

            int leftMargin = 30;
            int topMargin = 90;
            int spacing = 250;
            int comboWidth = 200;
            int comboHeight = 38;

            // Month ComboBox and Label
            var monthLabel = new Label()
            {
                Text = "Month:",
                Location = new Point(leftMargin, topMargin + 10),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(monthLabel);

            monthComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMargin + 70, topMargin),
                Size = new Size(comboWidth, comboHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            for (int i = 1; i <= 12; i++)
                monthComboBox.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            monthComboBox.SelectedIndexChanged += async (s, e) => await LoadAttendanceAsync();
            this.Controls.Add(monthComboBox);

            // Year ComboBox and Label
            var yearLabel = new Label()
            {
                Text = "Year:",
                Location = new Point(leftMargin + spacing, topMargin + 10),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(yearLabel);

            yearComboBox = new SiticoneComboBox()
            {
                Location = new Point(yearLabel.Location.X + 55, topMargin),
                Size = new Size(comboWidth, comboHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            for (int year = DateTime.Now.Year; year >= 2020; year--)
                yearComboBox.Items.Add(year.ToString());
            yearComboBox.SelectedItem = DateTime.Now.Year.ToString();
            yearComboBox.SelectedIndexChanged += async (s, e) => await LoadAttendanceAsync();
            this.Controls.Add(yearComboBox);

            // Sync Button
            syncBtn = new SiticoneButton()
            {
                Text = "🔄 Sync from Firebase",
                Location = new Point(leftMargin + spacing * 2, topMargin),
                Size = new Size(180, comboHeight),
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BorderRadius = 10,
                Cursor = Cursors.Hand
            };
            syncBtn.Click += async (s, e) => await LoadAttendanceAsync();
            this.Controls.Add(syncBtn);

            // DataGridView
            dgv = new SiticoneDataGridView()
            {
                Location = new Point(leftMargin, topMargin + 70),
                Size = new Size(this.ClientSize.Width - 2 * leftMargin, this.ClientSize.Height - (topMargin + 120)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Gainsboro,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            this.Controls.Add(dgv);

            // Summary label
            summaryLabel = new Label()
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = false,
                Size = new Size(500, 30),
                Location = new Point(leftMargin, this.ClientSize.Height - 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(summaryLabel);

            this.Resize += (s, e) =>
            {
                dgv.Size = new Size(this.ClientSize.Width - 2 * leftMargin, this.ClientSize.Height - (topMargin + 120));
                summaryLabel.Location = new Point(leftMargin, this.ClientSize.Height - 40);
            };

            // Initial load
            _ = LoadAttendanceAsync();
        }

        private async Task LoadStaffDirectory()
        {
            try
            {
                string url = "YOUR_API_KEY_HERE";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    staffDirectory = JsonConvert.DeserializeObject<Dictionary<string, StaffInfo>>(json);
                }
            }
            catch
            {
                staffDirectory = new Dictionary<string, StaffInfo>();
            }
        }

        private async Task LoadAttendanceAsync()
        {
            try
            {
                await LoadStaffDirectory();

                string firebaseUrl = "YOUR_API_KEY_HERE";
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(firebaseUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to fetch attendance data from Firebase.");
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var attendanceData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
                    if (attendanceData == null)
                    {
                        MessageBox.Show("No attendance data found.");
                        return;
                    }

                    int selectedMonth = monthComboBox.SelectedIndex + 1;
                    int selectedYear = int.Parse(yearComboBox.SelectedItem.ToString());

                    var records = new List<AllStaffAttendanceEntry>();

                    foreach (var dateEntry in attendanceData)
                    {
                        if (!DateTime.TryParse(dateEntry.Key, out DateTime dt)) continue;

                        if (dt.Month != selectedMonth || dt.Year != selectedYear) continue;

                        foreach (var staffEntry in dateEntry.Value)
                        {
                            string staffId = staffEntry.Key;
                            JToken token = staffEntry.Value == null ? null : JToken.FromObject(staffEntry.Value);

                            bool isPresent = false;
                            string time = "—";

                            if (token != null)
                            {
                                if (token.Type == JTokenType.Boolean)
                                {
                                    isPresent = token.Value<bool>();
                                }
                                else if (token.Type == JTokenType.Object)
                                {
                                    isPresent = token["present"]?.Value<bool>() == true;
                                    time = token["time"]?.ToString() ?? "—";
                                }
                            }

                            string staffName = staffDirectory != null && staffDirectory.TryGetValue(staffId, out StaffInfo info)
                                ? info.Name
                                : staffId;

                            records.Add(new AllStaffAttendanceEntry
                            {
                                Date = dt.ToString("dd MMM yyyy"),
                                StaffId = staffId,
                                StaffName = staffName,
                                Present = isPresent ? "Present" : "Absent",
                                Time = time
                            });
                        }
                    }

                    if (records.Count == 0)
                    {
                        MessageBox.Show("No attendance data found for the selected month/year.");
                    }

                    // Sort by Date, then StaffName
                    var sortedRecords = records.OrderBy(r => DateTime.Parse(r.Date)).ThenBy(r => r.StaffName).ToList();

                    dgv.DataSource = sortedRecords;

                    // Setup column headers and visibility
                    if (dgv.Columns["Date"] != null) dgv.Columns["Date"].HeaderText = "Date";
                    if (dgv.Columns["StaffName"] != null) dgv.Columns["StaffName"].HeaderText = "Staff Name";
                    if (dgv.Columns["StaffId"] != null)
                    {
                        dgv.Columns["StaffId"].HeaderText = "Staff ID";
                        dgv.Columns["StaffId"].Visible = false; // Hide ID column if you want cleaner UI
                    }
                    if (dgv.Columns["Present"] != null) dgv.Columns["Present"].HeaderText = "Attendance";
                    if (dgv.Columns["Time"] != null) dgv.Columns["Time"].HeaderText = "Time";

                    summaryLabel.Text = $"Total records: {records.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading attendance: " + ex.Message);
            }
        }

        private class StaffInfo
        {
            public string Name { get; set; }
            public string Role { get; set; }  // optional
        }

        private class AllStaffAttendanceEntry
        {
            public string Date { get; set; }
            public string StaffId { get; set; }
            public string StaffName { get; set; }
            public string Present { get; set; }
            public string Time { get; set; }
        }
    }
}
