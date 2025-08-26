using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class RevenueForm : UserControl
    {
        private SiticoneComboBox monthSelector;
        private SiticoneHtmlLabel feeCollectedLbl, feeRemainingLbl, paperLbl, otherLbl, admissionLbl, totalLbl;
        private SiticoneTextBox otherInput, paperInput;
        private SiticoneGradientButton saveBtn, editOtherBtn, editPaperBtn;
        private Chart chart;
        private SiticonePanel statsPanel;

        private string studentsFile = "Students.json";
        private string revenueFile = "Revenue.json";

        private Dictionary<string, RevenueEntry> revenueData;

        public RevenueForm()
        {
            InitializeComponent();
            LoadRevenueData();
            InitializeUI();
            LoadMonthData(DateTime.Now.ToString("yyyy-MM"));
        }

        private void LoadRevenueData()
        {
            if (!File.Exists(revenueFile))
                revenueData = new Dictionary<string, RevenueEntry>();
            else
                revenueData = JsonConvert.DeserializeObject<Dictionary<string, RevenueEntry>>(File.ReadAllText(revenueFile));
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Header
            var header = new SiticoneGradientPanel()
            {
                Dock = DockStyle.Top,
                Height = 60,
                FillColor = Color.FromArgb(52, 73, 94),
                FillColor2 = Color.FromArgb(108, 92, 231)
            };
            var headerLabel = new SiticoneHtmlLabel()
            {
                Text = "School Revenue Dashboard",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlignment = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(headerLabel);
            this.Controls.Add(header);

            // Stats Panel
            statsPanel = new SiticonePanel()
            {
                Dock = DockStyle.Left,
                Width = 350,
                Padding = new Padding(20),
                AutoScroll = true,
                ShadowDecoration = { Enabled = true, Depth = 10 }
            };
            this.Controls.Add(statsPanel);

            // Month Selector
            monthSelector = new SiticoneComboBox()
            {
                Width = 250,
                Location = new Point(20, 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            monthSelector.Items.AddRange(Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i).ToString("yyyy-MM"))
                .ToArray());
            monthSelector.SelectedIndex = 0;
            monthSelector.SelectedIndexChanged += (s, e) => LoadMonthData(monthSelector.SelectedItem.ToString());
            statsPanel.Controls.Add(monthSelector);

            int top = 60;
            feeCollectedLbl = CreateStatLabel("Fees Collected:", ref top);
            feeRemainingLbl = CreateStatLabel("Fees Remaining:", ref top);
            paperLbl = CreateStatLabel("Paper Money:", ref top);

            paperInput = CreateTextBox(ref top);
            editPaperBtn = CreateEditButton(ref top, (s, e) =>
            {
                EditValue(paperInput, "Enter Paper Money Amount");
            });

            otherLbl = CreateStatLabel("Other Revenue:", ref top);
            otherInput = CreateTextBox(ref top);
            editOtherBtn = CreateEditButton(ref top, (s, e) =>
            {
                EditValue(otherInput, "Enter Other Revenue Amount");
            });

            admissionLbl = CreateStatLabel("New Admissions:", ref top);
            totalLbl = CreateStatLabel("Total Revenue:", ref top);

            saveBtn = new SiticoneGradientButton()
            {
                Text = "💾 Save",
                Location = new Point(20, top + 20),
                Width = 250,
                Height = 40,
                FillColor = Color.FromArgb(41, 128, 185),
                FillColor2 = Color.FromArgb(108, 92, 231),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            saveBtn.Click += SaveBtn_Click;
            statsPanel.Controls.Add(saveBtn);

            // Chart Group Box
            var chartGroup = new SiticoneGroupBox()
            {
                Text = "📊 Revenue Overview",
                Dock = DockStyle.Top,
                Height = 300,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                CustomBorderColor = Color.FromArgb(108, 92, 231),
                ShadowDecoration = { Enabled = true, Depth = 10 },
                Padding = new Padding(10)
            };

            chart = new Chart()
            {
                Dock = DockStyle.Fill
            };
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.Series.Add(new Series("Revenue")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(46, 204, 113),
                XValueType = ChartValueType.String
            });

            chartGroup.Controls.Add(chart);

            // Add chart group box to the main control *above* stats and other content
            this.Controls.Add(chartGroup);
            this.Controls.SetChildIndex(chartGroup, 0); // Make sure it appears at the top

        }

        private SiticoneHtmlLabel CreateStatLabel(string text, ref int top)
        {
            var lbl = new SiticoneHtmlLabel()
            {
                Text = text,
                Location = new Point(20, top),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                Width = 250
            };
            statsPanel.Controls.Add(lbl);
            top += 30;
            return lbl;
        }

        private SiticoneTextBox CreateTextBox(ref int top)
        {
            var txt = new SiticoneTextBox()
            {
                Location = new Point(20, top),
                Width = 150,
                Enabled = false
            };
            statsPanel.Controls.Add(txt);
            return txt;
        }

        private SiticoneGradientButton CreateEditButton(ref int top, EventHandler clickHandler)
        {
            var btn = new SiticoneGradientButton()
            {
                Text = "✏️ Edit",
                Location = new Point(180, top),
                Width = 80,
                Height = 30,
                FillColor = Color.FromArgb(243, 156, 18),
                FillColor2 = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.Click += clickHandler;
            statsPanel.Controls.Add(btn);
            top += 40;
            return btn;
        }

        private void EditValue(SiticoneTextBox textBox, string title)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(title, "Edit Value", textBox.Text);
            if (int.TryParse(input, out int value))
            {
                textBox.Text = value.ToString();
            }
            else
            {
                MessageBox.Show("❌ Please enter a valid number.");
            }
        }

        private void LoadMonthData(string monthKey)
        {
            if (!File.Exists(studentsFile)) return;

            double totalCollected = 0;
            double totalAllFees = 0;
            int admissions = 0;

            string monthName = DateTime.ParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture)
                                       .ToString("MMMM", CultureInfo.InvariantCulture);

            // Match the actual JSON structure: Class -> StudentID -> Student
            var allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(File.ReadAllText(studentsFile));

            foreach (var classData in allClasses.Values)
            {
                foreach (var student in classData.Values)
                {
                    if (double.TryParse(student.Fee, out double fee) && !double.IsNaN(fee))
                    {
                        totalAllFees += fee;

                        if (student.FeeStatus != null &&
                            student.FeeStatus.TryGetValue(monthName, out bool isPaid) &&
                            isPaid)
                        {
                            totalCollected += fee;
                        }

                        if (!string.IsNullOrEmpty(student.AdmissionDate) &&
                            DateTime.TryParse(student.AdmissionDate, out DateTime admissionDate))
                        {
                            if (admissionDate.ToString("yyyy-MM") == monthKey)
                                admissions++;
                        }
                    }
                }
            }

            double totalRemaining = totalAllFees - totalCollected;

            if (!revenueData.ContainsKey(monthKey))
            {
                revenueData[monthKey] = new RevenueEntry
                {
                    MonthYear = monthKey,
                    FeesCollected = totalCollected,
                    FeesRemaining = totalRemaining,
                    PaperMoney = 0,
                    OtherRevenue = 0,
                    NewAdmissions = admissions
                };
            }
            else
            {
                revenueData[monthKey].FeesCollected = totalCollected;
                revenueData[monthKey].FeesRemaining = totalRemaining;
                revenueData[monthKey].NewAdmissions = admissions;
            }

            var entry = revenueData[monthKey];

            feeCollectedLbl.Text = $"Fees Collected: Rs. {entry.FeesCollected}";
            feeRemainingLbl.Text = $"Fees Remaining: Rs. {entry.FeesRemaining}";
            paperLbl.Text = "Paper Money:";
            paperInput.Text = entry.PaperMoney.ToString();
            otherLbl.Text = "Other Revenue:";
            otherInput.Text = entry.OtherRevenue.ToString();
            admissionLbl.Text = $"New Admissions: {entry.NewAdmissions}";

            double total = entry.FeesCollected + entry.PaperMoney + entry.OtherRevenue;
            totalLbl.Text = $"Total Revenue: Rs. {total:N0}";

            otherInput.Enabled = false;
            UpdateChart(revenueData.ToDictionary(
                kvp => kvp.Key,
                kvp => (kvp.Value.FeesCollected, kvp.Value.PaperMoney, kvp.Value.OtherRevenue)
            ));
        }


        private void SaveBtn_Click(object sender, EventArgs e)
        {
            string month = monthSelector.SelectedItem.ToString();

            if (!revenueData.ContainsKey(month))
                revenueData[month] = new RevenueEntry();

            revenueData[month].PaperMoney = int.TryParse(paperInput.Text, out int p) ? p : 0;
            revenueData[month].OtherRevenue = int.TryParse(otherInput.Text, out int o) ? o : 0;

            File.WriteAllText(revenueFile, JsonConvert.SerializeObject(revenueData, Formatting.Indented));
            MessageBox.Show("✅ Revenue data saved successfully.");
            LoadMonthData(month);
        }

        private void UpdateChart(Dictionary<string, (double FeesCollected, double PaperMoney, double OtherRevenue)> revenueData)
        {
            chart.Series.Clear();

            // Create series for each revenue type
            var feesSeries = new Series("Fees Collected")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(52, 152, 219), // Blue
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7
            };

            var paperSeries = new Series("Paper Money")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(46, 204, 113), // Green
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7
            };

            var otherSeries = new Series("Other Revenue")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(231, 76, 60), // Red
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7
            };

            // Add data to each series
            foreach (var month in revenueData.Keys)
            {
                feesSeries.Points.AddXY(month, revenueData[month].FeesCollected);
                paperSeries.Points.AddXY(month, revenueData[month].PaperMoney);
                otherSeries.Points.AddXY(month, revenueData[month].OtherRevenue);
            }

            // Add series to chart
            chart.Series.Add(feesSeries);
            chart.Series.Add(paperSeries);
            chart.Series.Add(otherSeries);

            // Configure chart area
            var chartArea = new ChartArea("MainArea")
            {
                AxisX = {
            Title = "Month",
            Interval = 1,
            MajorGrid = { LineColor = Color.LightGray },
            LabelStyle = { Angle = -45 }
        },
                AxisY = {
            Title = "Revenue (Rs.)",
            MajorGrid = { LineColor = Color.LightGray },
            Minimum = 0 // start from zero
        }
            };

            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(chartArea);

            // Auto adjust scale
            chart.ChartAreas[0].RecalculateAxesScale();

            // Enable legend
            chart.Legends.Clear();
            chart.Legends.Add(new Legend("MainLegend")
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center
            });
        }

    }
}
