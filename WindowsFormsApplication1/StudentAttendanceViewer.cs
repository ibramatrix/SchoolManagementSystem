using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms; // Add Siticone namespace

namespace WindowsFormsApplication1
{
    public partial class StudentAttendanceViewer : Form
    {
        private string studentId;
        private string className;
        private string studentName;
        private string studentContact;
        private string lastSavedPdfPath;
        private DataGridView dgv;
        private SiticoneComboBox monthComboBox;

        // Store all attendance fetched (date string -> present bool)
        private List<AttendanceRecord> allStudentAttendance = new List<AttendanceRecord>();

        public StudentAttendanceViewer(string id, string cls, string name, string contact)
        {
            studentId = id;
            className = cls;
            studentName = name;
            studentContact = contact;

            InitializeUI();

            LoadAttendance();
        }

        private void InitializeUI()
        {
            this.Text = $"Attendance - {studentName}";
            this.Size = new Size(900, 650);
            this.BackColor = Color.White;

            Label headerLabel = new Label
            {
                Text = $"Attendance for {studentName} ({className})",
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
                monthComboBox.Location = new Point((this.ClientSize.Width - monthComboBox.Width) / 2, 80);
                dgv.Location = new Point((this.ClientSize.Width - dgv.Width) / 2, 120);
            };

            // Siticone ComboBox for Month Selection
            monthComboBox = new SiticoneComboBox
            {
                Width = 200,
                Location = new Point((this.ClientSize.Width - 200) / 2, 80),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            monthComboBox.SelectedIndexChanged += MonthComboBox_SelectedIndexChanged;
            this.Controls.Add(monthComboBox);

            dgv = new DataGridView
            {
                Size = new Size(800, 400),
                Location = new Point((this.ClientSize.Width - 800) / 2, 120),
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

            this.Controls.Add(dgv);

            Label summaryLabel = new Label
            {
                Name = "summaryLabel",
                Text = "",
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(20, dgv.Bottom + 10),
                AutoSize = true
            };
            this.Controls.Add(summaryLabel);
        }

        private async void LoadAttendance()
        {
            Dictionary<string, Dictionary<string, bool>> allAttendance = null;

            try
            {
                allAttendance = await LoadAttendanceFromFirebaseAsync(className);
            }
            catch
            {
                // ignore and fallback
            }

            if (allAttendance == null || allAttendance.Count == 0)
            {
                string path = $"Attendance_{className}.json";
                if (!File.Exists(path))
                {
                    MessageBox.Show("No attendance data found online or locally.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var json = File.ReadAllText(path);
                allAttendance = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(json);
            }

            // Flatten attendance for this student into list of AttendanceRecord
            allStudentAttendance = allAttendance
                .Where(entry => entry.Value.ContainsKey(studentId))
                .Select(entry => new AttendanceRecord
                {
                    Date = DateTime.Parse(entry.Key),
                    Present = entry.Value[studentId]
                })
                .OrderByDescending(a => a.Date)
                .ToList();

            PopulateMonthComboBox();
            FilterAttendanceByMonth();
        }

        private void PopulateMonthComboBox()
        {
            monthComboBox.Items.Clear();

            var months = allStudentAttendance
                .Select(a => new { a.Date.Year, a.Date.Month })
                .Distinct()
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            foreach (var m in months)
            {
                string monthName = new DateTime(m.Year, m.Month, 1).ToString("MMMM yyyy");
                monthComboBox.Items.Add(monthName);
            }

            if (monthComboBox.Items.Count > 0)
                monthComboBox.SelectedIndex = 0;
        }

        private void MonthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAttendanceByMonth();
        }

        private void FilterAttendanceByMonth()
        {
            if (monthComboBox.SelectedIndex < 0) return;

            string selectedMonthYear = monthComboBox.SelectedItem.ToString();
            DateTime firstOfMonth = DateTime.ParseExact(selectedMonthYear, "MMMM yyyy", null);

            var filtered = allStudentAttendance
                .Where(a => a.Date.Year == firstOfMonth.Year && a.Date.Month == firstOfMonth.Month)
                .Select(a => new
                {
                    Date = a.Date.ToString("dd MMM yyyy"),
                    Present = a.Present ? "Present" : "Absent"
                })
                .ToList();

            dgv.DataSource = filtered;

            if (dgv.Columns.Count >= 2)
            {
                dgv.Columns[0].HeaderText = "Date";
                dgv.Columns[1].HeaderText = "Attendance";
            }

            int totalDays = filtered.Count;
            int presentDays = filtered.Count(a => a.Present == "Present");
            double percentage = totalDays == 0 ? 0 : (presentDays / (double)totalDays) * 100;

            var label = Controls.Find("summaryLabel", true).FirstOrDefault() as Label;
            if (label != null)
            {
                label.Text = $"Total Days: {totalDays}, Present: {presentDays}, Attendance: {percentage:F1}%";
            }
        }

        private async Task<Dictionary<string, Dictionary<string, bool>>> LoadAttendanceFromFirebaseAsync(string className)
        {
            string firebaseUrl = $"YOUR_API_KEY_HERE{className}.json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(firebaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(json) || json == "null")
                            return new Dictionary<string, Dictionary<string, bool>>();

                        return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(json);
                    }
                    else
                    {
                        MessageBox.Show("Failed to load attendance data from Firebase.");
                        return new Dictionary<string, Dictionary<string, bool>>();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to Firebase: {ex.Message}");
                    return new Dictionary<string, Dictionary<string, bool>>();
                }
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"{studentName.Replace(" ", "_")}_Attendance.pdf"
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

            doc.Add(new Paragraph($"Student Name: {studentName}\nClass: {className}\nContact: {studentContact}\n\n", bodyFont));

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.AddCell(new Phrase("Date", headerFont));
            table.AddCell(new Phrase("Status", headerFont));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                table.AddCell(new Phrase(row.Cells[0].Value?.ToString() ?? "", bodyFont));
                table.AddCell(new Phrase(row.Cells[1].Value?.ToString() ?? "", bodyFont));
            }

            doc.Add(table);
            doc.Close();

            lastSavedPdfPath = filePath;

            MessageBox.Show("PDF created successfully!\n\n(WhatsApp integration coming soon!)", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private class AttendanceRecord
        {
            public DateTime Date { get; set; }
            public bool Present { get; set; }
        }
    }
}
