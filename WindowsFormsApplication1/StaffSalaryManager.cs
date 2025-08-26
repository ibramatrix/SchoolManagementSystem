using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;
namespace WindowsFormsApplication1
{
    public partial class StaffSalaryManager : Form
    {
        private SiticoneComboBox monthComboBox;
        private SiticoneComboBox staffComboBox;
        private SiticoneTextBox baseSalaryTextBox;
        private SiticoneNumericUpDown absentDaysNumeric;
        private SiticoneTextBox calculatedSalaryTextBox;
        private SiticoneButton calculateBtn;
        private SiticoneButton printBtn;
        private Label headerLabel;

        private Dictionary<string, StaffInfo> staffDirectory;

        public StaffSalaryManager()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadStaffDirectoryAsync();
        }

        private void InitializeUI()
        {
            this.Text = "Staff Salary Manager";
            this.Size = new Size(800, 600);  // Increased width & height
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            headerLabel = new Label()
            {
                Text = "Staff Salary Manager",
                Font = new System.Drawing.Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };
            this.Controls.Add(headerLabel);

            int leftMargin = 50;
            int topMargin = 100;
            int labelWidth = 180;
            int controlWidth = 400;
            int controlHeight = 45;
            int verticalSpacing = 80;

            // Month
            var monthLabel = new Label()
            {
                Text = "Select Month:",
                Location = new Point(leftMargin, topMargin),
                Size = new Size(labelWidth, 30),
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(monthLabel);

            monthComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin - 5),
                Size = new Size(controlWidth, controlHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int i = 1; i <= 12; i++)
                monthComboBox.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            this.Controls.Add(monthComboBox);

            // Staff
            var staffLabel = new Label()
            {
                Text = "Select Staff:",
                Location = new Point(leftMargin, topMargin + verticalSpacing),
                Size = new Size(labelWidth, 30),
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(staffLabel);

            staffComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin + verticalSpacing - 5),
                Size = new Size(controlWidth, controlHeight),
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10,
                FillColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            staffComboBox.SelectedIndexChanged += StaffComboBox_SelectedIndexChanged;
            this.Controls.Add(staffComboBox);

            // Base Salary
            var salaryLabel = new Label()
            {
                Text = "Base Salary (PKR):",
                Location = new Point(leftMargin, topMargin + verticalSpacing * 2),
                Size = new Size(labelWidth, 30),
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(salaryLabel);

            baseSalaryTextBox = new SiticoneTextBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin + verticalSpacing * 2 - 5),
                Size = new Size(controlWidth, controlHeight),
                ReadOnly = true,
                Font = new System.Drawing.Font("Segoe UI", 12),
                FillColor = Color.White,
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10
            };
            this.Controls.Add(baseSalaryTextBox);

            // Absent Days
            var absentLabel = new Label()
            {
                Text = "Absent Days (Allowed 2):",
                Location = new Point(leftMargin, topMargin + verticalSpacing * 3),
                Size = new Size(labelWidth, 30),
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(absentLabel);

            absentDaysNumeric = new SiticoneNumericUpDown()
            {
                Location = new Point(leftMargin + labelWidth, topMargin + verticalSpacing * 3 - 5),
                Size = new Size(150, controlHeight),
                Minimum = 0,
                Maximum = 31,
                Value = 0,
                Font = new System.Drawing.Font("Segoe UI", 12),
                BorderRadius = 10
            };
            absentDaysNumeric.ValueChanged += AbsentDaysNumeric_ValueChanged;
            this.Controls.Add(absentDaysNumeric);

            // Calculated Salary
            var calculatedLabel = new Label()
            {
                Text = "Calculated Salary (PKR):",
                Location = new Point(leftMargin, topMargin + verticalSpacing * 4),
                Size = new Size(labelWidth, 30),
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(calculatedLabel);

            calculatedSalaryTextBox = new SiticoneTextBox()
            {
                Location = new Point(leftMargin + labelWidth, topMargin + verticalSpacing * 4 - 5),
                Size = new Size(controlWidth, controlHeight),
                ReadOnly = true,
                Font = new System.Drawing.Font("Segoe UI", 12),
                FillColor = Color.White,
                BorderColor = Color.FromArgb(41, 128, 185),
                BorderRadius = 10
            };
            this.Controls.Add(calculatedSalaryTextBox);

            // Buttons: side by side
            calculateBtn = new SiticoneButton()
            {
                Text = "Calculate Salary",
                Location = new Point(leftMargin + 30, topMargin + verticalSpacing * 5),
                Size = new Size(300, 50),
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                BorderRadius = 12,
                Cursor = Cursors.Hand
            };
            calculateBtn.Click += CalculateBtn_Click;
            this.Controls.Add(calculateBtn);

            printBtn = new SiticoneButton()
            {
                Text = "🖨 Print Salary Slip",
                Location = new Point(leftMargin + 350, topMargin + verticalSpacing * 5),
                Size = new Size(300, 50),
                FillColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                BorderRadius = 12,
                Cursor = Cursors.Hand
            };
            printBtn.Click += PrintBtn_Click;
            this.Controls.Add(printBtn);
        }


        private async Task LoadStaffDirectoryAsync()
        {
            try
            {
                string url = "YOUR_API_KEY_HERE";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    staffDirectory = JsonConvert.DeserializeObject<Dictionary<string, StaffInfo>>(json);
                }
                staffComboBox.Items.Clear();

                if (staffDirectory != null)
                {
                    foreach (var kvp in staffDirectory)
                    {
                        // Add staff key and display name in combo
                        staffComboBox.Items.Add(new ComboboxItem(kvp.Key, kvp.Value.Name));
                    }
                }

                if (staffComboBox.Items.Count > 0)
                    staffComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load staff list: " + ex.Message);
            }
        }

        private void StaffComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (staffComboBox.SelectedItem is ComboboxItem selected)
            {
                if (staffDirectory != null && staffDirectory.TryGetValue(selected.Key, out var staff))
                {
                    baseSalaryTextBox.Text = staff.Salary;
                    calculatedSalaryTextBox.Text = "";  // clear last calculated salary
                    absentDaysNumeric.Value = 0;
                }
            }
        }

        private void AbsentDaysNumeric_ValueChanged(object sender, EventArgs e)
        {
            calculatedSalaryTextBox.Text = "";  // reset calculation if absent days changes
        }

        private void CalculateBtn_Click(object sender, EventArgs e)
        {
            if (staffComboBox.SelectedItem is ComboboxItem selected)
            {
                if (staffDirectory == null || !staffDirectory.ContainsKey(selected.Key))
                {
                    MessageBox.Show("Select valid staff.");
                    return;
                }

                if (!decimal.TryParse(baseSalaryTextBox.Text, out decimal baseSalary))
                {
                    MessageBox.Show("Invalid base salary.");
                    return;
                }

                int allowedAbsent = 2; // as per your requirement
                int absentDays = (int)absentDaysNumeric.Value;

                if (absentDays > allowedAbsent)
                {
                    // Deduct salary proportionally for extra absent days
                    int extraAbsent = absentDays - allowedAbsent;
                    // Assuming salary is for full month of 30 days:
                    decimal deductionPerDay = baseSalary / 30m;
                    decimal deduction = deductionPerDay * extraAbsent;
                    decimal finalSalary = baseSalary - deduction;
                    if (finalSalary < 0) finalSalary = 0;
                    calculatedSalaryTextBox.Text = finalSalary.ToString("N2");
                }
                else
                {
                    // No deduction
                    calculatedSalaryTextBox.Text = baseSalary.ToString("N2");
                }
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            var selected = staffComboBox.SelectedItem as ComboboxItem;
            if (selected == null)
            {
                MessageBox.Show("Please select a staff member.");
                return;
            }

            if (string.IsNullOrEmpty(calculatedSalaryTextBox.Text))
            {
                MessageBox.Show("Please calculate the salary first.");
                return;
            }

            string staffName = staffDirectory[selected.Key].Name;
            string month = monthComboBox.SelectedItem?.ToString() ?? "";
            string baseSalary = baseSalaryTextBox.Text;
            string absentDays = absentDaysNumeric.Value.ToString();
            string finalSalary = calculatedSalaryTextBox.Text;
            string fatherName = staffDirectory[selected.Key].FatherName;
            string phoneNumber = staffDirectory[selected.Key].Phone.ToString();
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"{staffName}_SalarySlip_{month}.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportSalarySlipPdf(
                    saveFileDialog.FileName,
                    staffName,
                    fatherName,
                    phoneNumber,
                    month,
                    baseSalary,
                    absentDays,
                    finalSalary
                );
            }
        }

        private void ExportSalarySlipPdf(string path, string staffName, string fatherName, string phone, string month, string baseSalary, string absentDays, string finalSalary)
        {
            try
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLUE);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);

                // Title
                Paragraph title = new Paragraph("Salary Slip", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 25f
                };
                doc.Add(title);

                // Table with 2 columns
                PdfPTable table = new PdfPTable(2)
                {
                    WidthPercentage = 80,
                    SpacingBefore = 10f,
                    SpacingAfter = 20f,
                };
                table.SetWidths(new float[] { 1f, 2f });

                // Helper to create header cell
                PdfPCell CreateHeaderCell(string text)
                {
                    var cell = new PdfPCell(new Phrase(text, headerFont))
                    {
                        BackgroundColor = new BaseColor(41, 128, 185),
                        Padding = 8,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        BorderWidth = 0
                    };
                    return cell;
                }

                // Helper to create label cell
                PdfPCell CreateLabelCell(string text)
                {
                    var cell = new PdfPCell(new Phrase(text, labelFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Padding = 6,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        BorderWidth = 1
                    };
                    return cell;
                }

                // Helper to create value cell
                PdfPCell CreateValueCell(string text)
                {
                    var cell = new PdfPCell(new Phrase(text, valueFont))
                    {
                        Padding = 6,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        BorderWidth = 1
                    };
                    return cell;
                }

                // Add table headers
                table.AddCell(CreateHeaderCell("Field"));
                table.AddCell(CreateHeaderCell("Details"));

                // Add rows
                table.AddCell(CreateLabelCell("Staff Name"));
                table.AddCell(CreateValueCell(staffName));

                table.AddCell(CreateLabelCell("Father's Name"));
                table.AddCell(CreateValueCell(fatherName));

                table.AddCell(CreateLabelCell("Phone Number"));
                table.AddCell(CreateValueCell(phone));

                table.AddCell(CreateLabelCell("Month"));
                table.AddCell(CreateValueCell(month));

                table.AddCell(CreateLabelCell("Base Salary (PKR)"));
                table.AddCell(CreateValueCell(baseSalary));

                table.AddCell(CreateLabelCell("Absent Days"));
                table.AddCell(CreateValueCell(absentDays));

                table.AddCell(CreateLabelCell("Final Salary (PKR)"));
                table.AddCell(CreateValueCell(finalSalary));

                doc.Add(table);

                // Signature section aligned right
                Paragraph signature = new Paragraph("\n\nAuthorized Signature\n\n\n", labelFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                doc.Add(signature);

                doc.Close();

                MessageBox.Show("Salary slip PDF generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper class to hold key-value in combobox
        private class ComboboxItem
        {
            public string Key { get; }
            public string Value { get; }

            public ComboboxItem(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString() => Value;
        }

        private class StaffInfo
        {
            public string Address { get; set; }
            public string FatherName { get; set; }
            public string Manages { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public string Phone { get; set; }
            public string Salary { get; set; }
            public string Username { get; set; }
            public bool isActive { get; set; }
        }
    }
}
