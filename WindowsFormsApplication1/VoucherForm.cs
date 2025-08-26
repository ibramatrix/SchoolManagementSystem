using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class VoucherForm : Form
    {
        private Student student;
        private List<string> pendingMonths;
        private string selectedClass;

        private Dictionary<string, TextBox> feeInputs = new Dictionary<string, TextBox>();
        private Label totalLabel;
        private FlowLayoutPanel monthPanel;

        public VoucherForm(Student student, List<string> pendingMonths, string selectedClass)
        {
            this.student = student;
            this.pendingMonths = new List<string>(pendingMonths);
            this.selectedClass = selectedClass;

            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "📄 Fee Voucher";
            this.Size = new Size(600, 700);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label heading = new Label()
            {
                Text = "🧾 FEE VOUCHER",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Size = new Size(this.Width, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(heading);

            // Months
            Label monthsLbl = new Label
            {
                Text = "❗Pending Months:",
                Location = new Point(20, 60),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(monthsLbl);

            monthPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 85),
                Width = 540,
                Height = 60,
                AutoScroll = true
            };
            int currentMonthIndex = DateTime.Now.Month - 1;

            foreach (var month in pendingMonths)
            {
                int monthIndex = Array.IndexOf(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, month);

                // Only show months up to and including current month
                if (monthIndex >= 0 && monthIndex <= currentMonthIndex)
                {
                    var cb = new CheckBox()
                    {
                        Text = month,
                        Checked = true,
                        Tag = month,
                        Width = 100
                    };
                    cb.CheckedChanged += (s, e) => CalculateTotal();
                    monthPanel.Controls.Add(cb);
                }
            }

            this.Controls.Add(monthPanel);

            // Fees Table
            string[] fields = new string[]
            {
                "Student Name", "Father Name", "Class", "Fee", "Annual Charges",
                "Books", "Note Books", "Uniform", "Exams", "Absent Fine"
            };

            int top = monthPanel.Bottom + 20;
            for (int i = 0; i < fields.Length; i++)
            {
                Label lbl = new Label()
                {
                    Text = fields[i],
                    Location = new Point(30, top),
                    Size = new Size(150, 25)
                };
                this.Controls.Add(lbl);

                TextBox txt = new TextBox()
                {
                    Location = new Point(180, top),
                    Width = 150,
                    ReadOnly = fields[i] == "Student Name" || fields[i] == "Class"
                };

                // Autofill
                switch (fields[i])
                {
                    case "Student Name":
                        txt.Text = student.Name;
                        break;
                    case "Father Name":
                        txt.Text = student.FatherName ?? "";
                        break;
                    case "Class":
                        txt.Text = selectedClass;
                        break;
                    case "Fee":
                        txt.Text = student.Fee;
                        txt.ReadOnly = true;
                        break;
                    default:
                        txt.Text = "0";
                        txt.TextChanged += (s, e) => CalculateTotal();
                        break;
                }


                feeInputs[fields[i]] = txt;
                this.Controls.Add(txt);

                top += 35;
            }

            // Total
            Label totalText = new Label()
            {
                Text = "Total:",
                Location = new Point(30, top),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            totalLabel = new Label()
            {
                Text = "Rs. 0",
                Location = new Point(180, top),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };
            this.Controls.Add(totalText);
            this.Controls.Add(totalLabel);

            // Print Button
            Button printBtn = new Button()
            {
                Text = "🖨 Print Voucher",
                Location = new Point(180, top + 40),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            printBtn.Click += (s, e) => PrintVoucher();
            this.Controls.Add(printBtn);

            CalculateTotal();
        }

        private void CalculateTotal()
        {
            int monthCount = monthPanel.Controls.OfType<CheckBox>().Count(cb => cb.Checked);
            double perMonthFee = double.TryParse(student.Fee, out var f) ? f : 0;

            double otherFees = 0;
            foreach (var field in feeInputs)
            {
                if (field.Key == "Student Name" || field.Key == "Class" || field.Key == "Father Name" || field.Key == "Fee") continue;

                if (double.TryParse(field.Value.Text.Trim(), out double val))
                    otherFees += val;
            }

            double total = (monthCount * perMonthFee) + otherFees;
            totalLabel.Text = $"Rs. {total:N0}";
        }

        private void PrintVoucher()
        {
            MessageBox.Show("🖨 Printed! (Replace this with actual PDF or Printer Code)");
            // Optional: You can implement iTextSharp to export the table to PDF
        }
    }
}
