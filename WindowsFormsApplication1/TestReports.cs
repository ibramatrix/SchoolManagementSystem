using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace WindowsFormsApplication1
{
    public partial class TestReports : Form
    {
        private string studentId;
        private string className;
        private string studentName;
        private DataGridView dgv;
        private string lastSavedPdfPath;

        public TestReports(string id, string cls, string name)
        {
            InitializeComponent();


            studentId = id;
            className = cls;
            studentName = name;
            Console.WriteLine(studentId + " " + className + " " + studentName);

            InitializeUI();
            LoadReports();
        }
        private void InitializeUI()
        {
            this.Text = $"Report Card - {studentName}";
            this.Size = new Size(900, 600);
            this.BackColor = Color.White;

            // Header label at the top
            Label titleLabel = new Label()
            {
                Text = $"Report Card for {studentName} ({className})",
                Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Size = new Size(this.Width, 60),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);
            int padding = 20;

            // Printer Button (top-right corner)
            Button printBtn = new Button
            {
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                FlatAppearance = { BorderSize = 0 },
                Location = new Point(this.ClientSize.Width - 100 - padding, this.ClientSize.Height - 40 - padding)

            };

            try
            {
                printBtn.Image = System.Drawing.Image.FromFile("printer.png");
            }
            catch
            {
                printBtn.Text = "🖨"; // fallback text
            }

            printBtn.Click += PrintBtn_Click;
            this.Controls.Add(printBtn);

            // Keep it top-right on resize
            printBtn.Location = new Point(
       this.ClientSize.Width - printBtn.Width - padding,
       this.ClientSize.Height - printBtn.Height - padding
   );

            this.Resize += (s, e) =>
            {
                printBtn.Location = new Point(
                    this.ClientSize.Width - printBtn.Width - padding,
                    this.ClientSize.Height - printBtn.Height - padding
                );
            };

            // Create DataGridView
            dgv = new DataGridView
            {
                Size = new Size(700, 250),
                Location = new Point((this.ClientSize.Width - 700) / 2, 200), // Bottom-center
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

            // Make sure it stays centered on resize
            this.Resize += (s, e) =>
            {
                dgv.Location = new Point((this.ClientSize.Width - dgv.Width) / 2, 200);
            };

            this.Controls.Add(dgv);
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"{studentName.Replace(" ", "_")}_ReportCard.pdf"
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

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            var subFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            // 1. School Header
            Paragraph header = new Paragraph("UNIVERSAL SCHOOL SYSTEM", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            doc.Add(header);

            // 2. Student Info
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingAfter = 20;

            infoTable.AddCell(new Phrase("Student Name:", subFont));
            infoTable.AddCell(new Phrase(studentName, regularFont));
            infoTable.AddCell(new Phrase("Class/Section:", subFont));
            infoTable.AddCell(new Phrase(className, regularFont));
            infoTable.AddCell(new Phrase("Father's Name:", subFont));
            infoTable.AddCell(new Phrase("__________", regularFont));
            infoTable.AddCell(new Phrase("Address:", subFont));
            infoTable.AddCell(new Phrase("__________", regularFont));

            doc.Add(infoTable);

            // 3. Test Results Table
            PdfPTable testTable = new PdfPTable(4);
            testTable.WidthPercentage = 100;
            testTable.SetWidths(new float[] { 3, 1, 1, 1 });
            testTable.SpacingAfter = 20;

            // Table Header
            string[] headers = { "Test Name", "Obtained", "Total", "Grade" };
            foreach (var h in headers)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(h, subFont))
                {
                    BackgroundColor = new BaseColor(220, 220, 220),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                testTable.AddCell(headerCell);
            }

            // Table Rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                string testName = row.Cells["TestName"]?.Value?.ToString() ?? "";
                string obtained = row.Cells["ObtainedMarks"]?.Value?.ToString() ?? "0";
                string total = row.Cells["TotalMarks"]?.Value?.ToString() ?? "0";
                string percentStr = row.Cells["Percentage"]?.Value?.ToString()?.Replace("%", "") ?? "0";

                // Grade based on percentage
                string grade = "F";
                if (double.TryParse(percentStr, out double percent))
                {
                    if (percent >= 85) grade = "A+";
                    else if (percent >= 70) grade = "A";
                    else if (percent >= 60) grade = "B";
                    else if (percent >= 50) grade = "C";
                    else grade = "F";
                }

                testTable.AddCell(new PdfPCell(new Phrase(testName, regularFont)));
                testTable.AddCell(new PdfPCell(new Phrase(obtained, regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                testTable.AddCell(new PdfPCell(new Phrase(total, regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                testTable.AddCell(new PdfPCell(new Phrase(grade, regularFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            doc.Add(testTable);

            // 4. Teacher's Feedback
            Paragraph feedback = new Paragraph("Teacher's Feedback:\n\n\n_____________________________\n\n\n", regularFont)
            {
                SpacingAfter = 10
            };
            doc.Add(feedback);

            // 5. Total School Days / Attendance
            PdfPTable attendanceTable = new PdfPTable(2);
            attendanceTable.WidthPercentage = 60;
            attendanceTable.SetWidths(new float[] { 1, 1 });
            attendanceTable.SpacingAfter = 40;

            attendanceTable.AddCell(new Phrase("Total School Days:", subFont));
            attendanceTable.AddCell(new Phrase("__________", regularFont));
            attendanceTable.AddCell(new Phrase("Attendance:", subFont));
            attendanceTable.AddCell(new Phrase("__________", regularFont));

            doc.Add(attendanceTable);

            // 6. Signature
            Paragraph signature = new Paragraph("\n\nTeacher's Signature: ____________________", subFont)
            {
                SpacingBefore = 30
            };
            doc.Add(signature);

            doc.Close();
            lastSavedPdfPath = filePath; // Save path for screenshot

            DialogResult result = MessageBox.Show(
    "Do you want to send this report card via WhatsApp?",
    "Send to WhatsApp",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendWhatsAppReport();
            }

        }



        private async void LoadReports()
        {
            try
            {
                string firebaseUrl = $"YOUR_API_KEY_HERE{className}.json";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(firebaseUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to fetch reports from Firebase.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(json) || json == "null")
                    {
                        MessageBox.Show("No test reports found for this class.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    AllTestReports allReports = JsonConvert.DeserializeObject<AllTestReports>(json);

                    string lookupId = studentId;

                    var reportList = allReports
                        .Where(r => r.Value.Students != null && r.Value.Students.ContainsKey(lookupId))
                        .Select(r =>
                        {
                            var s = r.Value.Students[lookupId];
                            double percentage = Math.Round((s.ObtainedMarks / (double)r.Value.TotalMarks) * 100, 2);

                            return new
                            {
                                TestNo = r.Key,
                                TestName = r.Value.TestName,
                                TotalMarks = r.Value.TotalMarks,
                                ObtainedMarks = s.ObtainedMarks,
                                Percentage = percentage + "%"
                            };
                        }).ToList();

                    if (reportList.Count == 0)
                    {
                        MessageBox.Show($"No report found for student ID: {lookupId}", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    dgv.DataSource = reportList;

                    // Color-code rows based on percentage
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.Cells["Percentage"].Value != null)
                        {
                            string percentStr = row.Cells["Percentage"].Value.ToString().Replace("%", "");
                            if (double.TryParse(percentStr, out double percent))
                            {
                                if (percent < 50)
                                    row.DefaultCellStyle.ForeColor = Color.Red;
                                else if (percent >= 85)
                                    row.DefaultCellStyle.ForeColor = Color.Green;
                                else
                                    row.DefaultCellStyle.ForeColor = Color.Black;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reports from Firebase:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SendWhatsAppReport()
        {
            try
            {
                string imgPath = Path.Combine(Application.StartupPath, "report_card.png");

                // ✅ Create image from PDF
                using (var document = PdfiumViewer.PdfDocument.Load(lastSavedPdfPath))
                {
                    using (var img = document.Render(0, 800, 1000, true))
                    {
                        img.Save(imgPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                // ✅ Run Python script to send image
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python", // or full path to python.exe
                    Arguments = $"send_report.py \"{imgPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                        MessageBox.Show("WhatsApp error:\n" + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show("Report card sent via WhatsApp successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong:\n" + ex.Message, "WhatsApp Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
