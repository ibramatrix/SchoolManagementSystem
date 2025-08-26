using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApplication1
{

    public partial class StaffAttendanceViewer : Form
    {
        private string staffId;
        private string staffName;
        private string lastSavedPdfPath;
        private DataGridView dgv;
        private ComboBox monthComboBox;
        private ComboBox yearComboBox;
        private bool isInitialized = false;

        public StaffAttendanceViewer(string staffId, string staffName)
        {
            this.staffId = staffId;
            this.staffName = staffName;
            MessageBox.Show("Loaded for staffId = " + staffId);

            InitializeComponent();  // Designer components
            InitializeUI();         // Your custom UI setup (including dgv)

            this.Load += StaffAttendanceViewer_Load;  // ✅ Use form Load event instead
        }
        private void StaffAttendanceViewer_Load(object sender, EventArgs e)
        {
            LoadAttendance(); // ✅ Safe to call now — dgv is definitely initialized
        }


        private void InitializeUI()
        {
            this.Text = $"Attendance - {staffName}";
            this.Size = new Size(900, 600);
            this.BackColor = Color.White;

            yearComboBox = new ComboBox
            {
                Location = new Point(30, 80),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int year = DateTime.Now.Year; year >= 2020; year--)
                yearComboBox.Items.Add(year);
            yearComboBox.SelectedIndexChanged += FilterChanged;
            this.Controls.Add(yearComboBox);

            monthComboBox = new ComboBox
            {
                Location = new Point(150, 80),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int i = 1; i <= 12; i++)
                monthComboBox.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
            monthComboBox.SelectedIndexChanged += FilterChanged;
            this.Controls.Add(monthComboBox);

            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            yearComboBox.SelectedItem = DateTime.Now.Year;

            Button syncBtn = new Button
            {
                Text = "🔄 Sync from Firebase",
                Location = new Point(300, 80),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            syncBtn.Click += SyncBtn_Click;
            this.Controls.Add(syncBtn);

            Button editBtn = new Button
            {
                Text = "✏️ Edit Attendance",
                Location = new Point(480, 80),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            editBtn.Click += EditBtn_Click;
            this.Controls.Add(editBtn);

            Label headerLabel = new Label
            {
                Text = $"Attendance for {staffName} (Staff)",
                Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Size = new Size(this.Width, 60),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(headerLabel);

            Button printBtn = new Button
            {
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Location = new Point(this.ClientSize.Width - 60, 20),
                FlatAppearance = { BorderSize = 0 },
                Text = "🖨"
            };
            printBtn.Click += PrintBtn_Click;
            this.Controls.Add(printBtn);
            this.Resize += (s, e) =>
            {
                printBtn.Location = new Point(this.ClientSize.Width - 60, 20);
            };

            dgv = new DataGridView
            {
                Size = new Size(800, 400),
                Location = new Point((this.ClientSize.Width - 800) / 2, 100),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Gainsboro,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Segoe UI", 10),
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(236, 240, 241),
                    SelectionForeColor = Color.Black
                },
                EnableHeadersVisualStyles = false
            };
            this.Resize += (s, e) =>
            {
                dgv.Location = new Point((this.ClientSize.Width - dgv.Width) / 2, 100);
            };
            this.Controls.Add(dgv);

            Label summaryLabel = new Label
            {
                Name = "summaryLabel",
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 400) / 2, 510)
            };
            this.Controls.Add(summaryLabel);


            // Set combo values AFTER everything is initialized (no event should fire until now)
            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            yearComboBox.SelectedItem = DateTime.Now.Year;

            isInitialized = true;


            MessageBox.Show("✅ dgv has been initialized.");

        }

        private async void LoadAttendance()
        {
            if (dgv == null)
            {
                MessageBox.Show("⚠️ DataGridView is not initialized.");
                return;
            }

            try
            {
                string firebaseUrl = "YOUR_API_KEY_HERE";
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(firebaseUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("❌ Failed to fetch attendance from Firebase.");
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var attendanceData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                    if (attendanceData == null)
                    {
                        MessageBox.Show("⚠️ Attendance data is null.");
                        return;
                    }

                    int selectedMonth = monthComboBox?.SelectedIndex + 1 ?? DateTime.Now.Month;
                    int selectedYear = yearComboBox?.SelectedItem != null ? Convert.ToInt32(yearComboBox.SelectedItem) : DateTime.Now.Year;

                    var filtered = attendanceData
                        .Where(entry =>
                        {
                            if (!DateTime.TryParse(entry.Key, out DateTime dt)) return false;
                            return dt.Month == selectedMonth && dt.Year == selectedYear &&
                                   entry.Value != null && entry.Value.ContainsKey(staffId);
                        })
                        .OrderByDescending(entry => entry.Key)
                        .Select(entry =>
                        {
                            string date = entry.Key;
                            var rawValue = entry.Value[staffId];
                            var token = JToken.FromObject(rawValue);

                            bool isPresent = false;
                            string time = "—";

                            if (token.Type == JTokenType.Boolean)
                            {
                                isPresent = token.Value<bool>();
                            }
                            else if (token.Type == JTokenType.Object)
                            {
                                isPresent = token["present"]?.Value<bool>() == true;
                                time = token["time"]?.ToString() ?? "—";
                            }

                            return new AttendanceEntry
                            {
                                Date = DateTime.Parse(date).ToString("dd MMM yyyy"),
                                Present = isPresent ? "Present" : "Absent",
                                Time = isPresent ? time : "—"
                            };
                        })
                        .ToList();

                    dgv.DataSource = null;
                    dgv.DataSource = filtered;
                    dgv.Visible = true;
                    dgv.BringToFront();
                    dgv.Refresh();

                    if (dgv.Columns.Count >= 3)
                    {
                        dgv.Columns[0].HeaderText = "Date";
                        dgv.Columns[1].HeaderText = "Attendance";
                        dgv.Columns[2].HeaderText = "Time";
                    }

                    int total = filtered.Count;
                    int presentCount = filtered.Count(a => a.Present == "Present");
                    double percent = total > 0 ? (presentCount / (double)total) * 100 : 0;

                    var summaryLabel = Controls.Find("summaryLabel", true).FirstOrDefault() as Label;
                    if (summaryLabel != null)
                    {
                        summaryLabel.Text = total == 0
                            ? "No attendance data available."
                            : $"Total Days: {total}   Present: {presentCount}   Attendance: {percent:F1}%";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading attendance: " + ex.Message);
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;  // 🛡️ Prevent premature call
            LoadAttendance();
        }

        private async void SyncBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string firebaseUrl = $"YOUR_API_KEY_HERE";
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(firebaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        File.WriteAllText("Attendance_Staff.json", json);
                        MessageBox.Show("✅ Synced from Firebase!");
                        LoadAttendance();
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to fetch from Firebase");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("❗Please select a row to edit.");
                return;
            }

            var selectedRow = dgv.SelectedRows[0];
            string date = selectedRow.Cells["Date"].Value?.ToString();
            string status = selectedRow.Cells["Attendance"].Value?.ToString(); // <-- FIXED

            DialogResult result = MessageBox.Show(
                $"Change attendance for {date}?\nCurrently marked: {status}",
                "Edit Attendance",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool newStatus = status != "Present";
                UpdateAttendance(date, newStatus);
            }
        }


        private async void UpdateAttendance(string dateStr, bool isPresent)
        {
            try
            {
                string path = "Attendance_Staff.json";
                var attendanceData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(path));
                string firebaseDateKey = DateTime.Parse(dateStr).ToString("yyyy-MM-dd");

                if (!attendanceData.ContainsKey(firebaseDateKey))
                    attendanceData[firebaseDateKey] = new Dictionary<string, bool>();

                attendanceData[firebaseDateKey][staffId] = isPresent;

                string firebasePath = $"YOUR_API_KEY_HERE{firebaseDateKey}/{staffId}.json";

                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(isPresent), Encoding.UTF8, "application/json");
                    var result = await client.PutAsync(firebasePath, content);

                    if (result.IsSuccessStatusCode)
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(attendanceData, Formatting.Indented));
                        MessageBox.Show("✅ Attendance updated!");
                        LoadAttendance();
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to update Firebase");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"{staffName.Replace(" ", "_")}_Attendance.pdf"
            };

            if (save.ShowDialog() == DialogResult.OK)
            {
                ExportToPdf(save.FileName);
            }
        }

        private void ExportToPdf(string filePath)
        {
            Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            doc.Add(new Paragraph("UNIVERSAL SCHOOL SYSTEM", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            });

            doc.Add(new Paragraph($"Staff Name: {staffName}\nStaff ID: {staffId}\n\n", bodyFont));

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.AddCell(new Phrase("Date", headerFont));
            table.AddCell(new Phrase("Status", headerFont));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                table.AddCell(new Phrase(row.Cells["Date"].Value?.ToString(), bodyFont));
                table.AddCell(new Phrase(row.Cells["Attendance"].Value?.ToString(), bodyFont));
            }

            doc.Close();
            lastSavedPdfPath = filePath;

            MessageBox.Show("✅ PDF generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
