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
using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text;
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System.Runtime.ConstrainedExecution;
using CefSharp;
using CefSharp.WinForms;
using Microsoft.VisualBasic;


namespace WindowsFormsApplication1
{
    public partial class ExpenseTracker : Form
    {
        private LiveCharts.WinForms.PieChart pieChart;
        private Label lblElectricity, lblSalaries, lblStationery, lblCleaning, lblTotal;
        private ProgressBar loadingBar;
        private Label statusLabel;
        private int stationery = 5000;
        private int cleaning = 3000;

        public ExpenseTracker()
        {
            InitializeComponent();

            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1"); // Disable GPU acceleration
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp.BrowserSubprocess.exe");

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            InitializeUI();
            this.Load += ExpenseDashboardForm_Load;
        }

        private async void ExpenseDashboardForm_Load(object sender, EventArgs e)
        {
            await LoadWithProgress();
        }

        private void InitializeUI()
        {
            this.Text = "School Expense Dashboard";
            this.Size = new Size(1000, 600);
            this.BackColor = Color.White;

            loadingBar = new ProgressBar
            {
                Location = new Point(300, 250),
                Size = new Size(400, 30),
                Style = ProgressBarStyle.Marquee,
                Visible = true
            };
            Controls.Add(loadingBar);

            lblElectricity = CreateLabel("Electricity Bill", 50);
            lblSalaries = CreateLabel("Salaries", 100);
            lblStationery = CreateLabel("Stationery", 150);
            lblCleaning = CreateLabel("Cleaning", 200);
            lblTotal = CreateLabel("Total Expenses", 260, FontStyle.Bold);

            pieChart = new LiveCharts.WinForms.PieChart
            {
                Location = new Point(500, 100),
                Size = new Size(400, 300),
                Visible = false
            };
            this.Controls.Add(pieChart);

            Button btnExportPdf = new Button { Text = "Export as PDF", Location = new Point(50, 320), Visible = false };
            btnExportPdf.Click += BtnExportPdf_Click;
            Controls.Add(btnExportPdf);

            statusLabel = new Label
            {
                Location = new Point(300, 280),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                Text = "Initializing..."
            };
            Controls.Add(statusLabel);

            // Edit button for Stationery
            Button btnEditStationery = new Button
            {
                Text = "Edit",
                Location = new Point(250, 150),
                Height = lblStationery.Height,
                Width = 60,
                Visible = false,
                Name = "btnEditStationery"
            };
            btnEditStationery.Click += (s, e) => EditExpense("Stationery");
            Controls.Add(btnEditStationery);

            // Edit button for Cleaning
            Button btnEditCleaning = new Button
            {
                Text = "Edit",
                Location = new Point(250, 200),
                Height = lblCleaning.Height,
                Width = 60,
                Visible = false,
                Name = "btnEditCleaning"
            };
            btnEditCleaning.Click += (s, e) => EditExpense("Cleaning");
            Controls.Add(btnEditCleaning);
        }





        private void SaveExpenseToJson(int electricity, int salaries, int stationery, int cleaning)
        {
            string filePath = "Expenses.json";
            Dictionary<string, Dictionary<string, MonthlyExpense>> data;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MonthlyExpense>>>(json)
                       ?? new Dictionary<string, Dictionary<string, MonthlyExpense>>();
            }
            else
            {
                data = new Dictionary<string, Dictionary<string, MonthlyExpense>>();
            }

            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.ToString("MMMM");

            if (!data.ContainsKey(year))
                data[year] = new Dictionary<string, MonthlyExpense>();

            // Check if already saved and same values
            if (data[year].ContainsKey(month))
            {
                var existing = data[year][month];
                if (existing.Electricity == electricity &&
                    existing.Salaries == salaries &&
                    existing.Stationery == stationery &&
                    existing.Cleaning == cleaning)
                {
                    Console.WriteLine("No changes, skipping update for this month.");
                    return; // Already saved and values haven't changed
                }
            }

            // Save or overwrite the current month
            data[year][month] = new MonthlyExpense
            {
                Electricity = electricity,
                Salaries = salaries,
                Stationery = stationery,
                Cleaning = cleaning
            };

            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }


        private void EditExpense(string type)
        {
            string prompt = $"Enter new amount for {type}:";
            string input = Microsoft.VisualBasic.Interaction.InputBox(prompt, "Edit Expense", type == "Stationery" ? stationery.ToString() : cleaning.ToString());

            if (int.TryParse(input, out int newValue))
            {
                int electricity = int.TryParse(lblElectricity.Text.Split(' ').Last(), out int e) ? e : 0;
                int salaries = LoadTotalSalaries(); // Always reload for consistency

                if (type == "Stationery")
                {
                    stationery = newValue;
                    lblStationery.Text = $"Stationery: Rs {stationery}";
                }
                else
                {
                    cleaning = newValue;
                    lblCleaning.Text = $"Cleaning: Rs {cleaning}";
                }

                UpdateChart();
                UpdateTotal();

                // Save updated values to Expenses.json
                SaveExpenseToJson(electricity, salaries, stationery, cleaning);
            }
            else
            {
                MessageBox.Show("Invalid value entered.");
            }
        }


        private void UpdateChart()
        {
            pieChart.Series = new SeriesCollection
    {
        new PieSeries { Title = "Salaries", Values = new ChartValues<int> { LoadTotalSalaries() }, Fill = System.Windows.Media.Brushes.SkyBlue },
        new PieSeries { Title = "Electricity", Values = new ChartValues<int> { int.TryParse(lblElectricity.Text.Split(' ').Last(), out int e) ? e : 0 }, Fill = System.Windows.Media.Brushes.Orange },
        new PieSeries { Title = "Stationery", Values = new ChartValues<int> { stationery }, Fill = System.Windows.Media.Brushes.LightGreen },
        new PieSeries { Title = "Cleaning", Values = new ChartValues<int> { cleaning }, Fill = System.Windows.Media.Brushes.Gray }
    };
        }

        private void UpdateTotal()
        {
            int electricity = int.TryParse(lblElectricity.Text.Split(' ').Last(), out int e) ? e : 0;
            int salaries = LoadTotalSalaries();
            int total = electricity + salaries + stationery + cleaning;
            lblTotal.Text = $"Total Expenses: Rs {total}";
        }

        private Label CreateLabel(string text, int y, FontStyle style = FontStyle.Regular)
        {
            Label lbl = new Label
            {
                Text = text + ": Rs 0",
                Font = new System.Drawing.Font("Segoe UI", 12, style),
                Location = new Point(50, y),
                AutoSize = true,
                Visible = false
            };
            Controls.Add(lbl);
            return lbl;
        }

        private async Task LoadWithProgress()
        {
            try
            {
                UpdateStatus("Launching browser for bill...", 10);
                await Task.Delay(500);

                int electricity = await ShowFescoBrowserAndGetBill();
                UpdateStatus("Bill loaded successfully", 40);
                CompareWithPreviousElectricity(electricity);

                int salaries = LoadTotalSalaries();
                UpdateStatus("Salaries calculated", 60);

                int total = electricity + salaries + stationery + cleaning;

                lblElectricity.Text = $"Electricity Bill: Rs {electricity}";
                lblSalaries.Text = $"Salaries: Rs {salaries}";
                lblStationery.Text = $"Stationery: Rs {stationery}";
                lblCleaning.Text = $"Cleaning: Rs {cleaning}";
                lblTotal.Text = $"Total Expenses: Rs {total}";

                UpdateStatus("Generating chart", 80);

                pieChart.Series = new SeriesCollection
                {
                    new PieSeries { Title = "Salaries", Values = new ChartValues<int> { salaries }, Fill = System.Windows.Media.Brushes.SkyBlue },
                    new PieSeries { Title = "Electricity", Values = new ChartValues<int> { electricity }, Fill = System.Windows.Media.Brushes.Orange },
                    new PieSeries { Title = "Stationery", Values = new ChartValues<int> { stationery }, Fill = System.Windows.Media.Brushes.LightGreen },
                    new PieSeries { Title = "Cleaning", Values = new ChartValues<int> { cleaning }, Fill = System.Windows.Media.Brushes.Gray }
                };

                SaveExpenseToJson(electricity, salaries, stationery, cleaning);

                // Hide loading UI
                loadingBar.Visible = false;
                statusLabel.Visible = false;
                Controls.Remove(loadingBar);
                Controls.Remove(statusLabel);
                loadingBar.Dispose();
                statusLabel.Dispose();

                // Show main UI
                lblElectricity.Visible = true;
                lblSalaries.Visible = true;
                lblStationery.Visible = true;
                lblCleaning.Visible = true;
                lblTotal.Visible = true;
                pieChart.Visible = true;

                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Button btn) btn.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                loadingBar.Visible = false;
                statusLabel.Text = "Failed to load.";
            }
        }





        private async Task LoadExpenseDashboard()
        {
            int electricity = await ShowFescoBrowserAndGetBill();

            int salaries = LoadTotalSalaries();
            int stationery = 5000;
            int cleaning = 3000;

            int total = electricity + salaries + stationery + cleaning;

            lblElectricity.Text = $"Electricity Bill: Rs {electricity}";
            lblSalaries.Text = $"Salaries: Rs {salaries}";
            lblStationery.Text = $"Stationery: Rs {stationery}";
            lblCleaning.Text = $"Cleaning: Rs {cleaning}";
            lblTotal.Text = $"Total Expenses: Rs {total}";

            pieChart.Series = new SeriesCollection
        {
            new PieSeries { Title = "Salaries", Values = new ChartValues<int> { salaries }, Fill = System.Windows.Media.Brushes.SkyBlue },
            new PieSeries { Title = "Electricity", Values = new ChartValues<int> { electricity }, Fill = System.Windows.Media.Brushes.Orange },
            new PieSeries { Title = "Stationery", Values = new ChartValues<int> { stationery }, Fill = System.Windows.Media.Brushes.LightGreen },
            new PieSeries { Title = "Cleaning", Values = new ChartValues<int> { cleaning }, Fill = System.Windows.Media.Brushes.Gray }
        };

            SaveExpenseToJson(electricity, salaries, stationery, cleaning);
        }

        private async Task<int> ShowFescoBrowserAndGetBill()
        {
            int bill = 0;

            await Task.Run(() =>
            {
                this.Invoke(new Action(() =>
                {
                    using (var browserForm = new FescoBillBrowser("27135115146900"))
                    {
                        var result = browserForm.ShowDialog();
                        if (result == DialogResult.OK && browserForm.Tag != null)
                        {
                            bill = Convert.ToInt32(browserForm.Tag);
                            if (bill < 0)
                            {
                                Console.WriteLine("DEKHA JAYE GAA");
                            }
                        }
                    }
                }));
            });

            return bill;
        }



        private void UpdateStatus(string message, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(message, progress)));
                return;
            }

            statusLabel.Text = message;
            loadingBar.Value = Math.Min(progress, 100);
        }





        private int LoadTotalSalaries()
        {
            try
            {
                string json = File.ReadAllText("Staff.json");
                var data = JsonConvert.DeserializeObject<Dictionary<string, Staff>>(json);
                int total = 0;
                foreach (var s in data.Values)
                {
                    if (int.TryParse(s.Salary, out int sal))
                        total += sal;
                }
                return total;
            }
            catch
            {
                MessageBox.Show("Could not load staff salaries.");
                return 0;
            }
        }


        private void CompareWithPreviousElectricity(int currentBill)
        {
            string path = "Expenses.json";
            if (!File.Exists(path)) return;

            var allData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MonthlyExpense>>>(File.ReadAllText(path));
            var year = DateTime.Now.Year.ToString();
            var prevMonth = DateTime.Now.AddMonths(-1).ToString("MMMM");

            if (allData.ContainsKey(year) && allData[year].ContainsKey(prevMonth))
            {
                int previousBill = allData[year][prevMonth].Electricity;
                int diff = currentBill - previousBill;
                MessageBox.Show($"Previous month electricity bill: Rs {previousBill}\nDifference: Rs {diff}", "Electricity Bill Comparison");
            }
        }



        private void SaveSnapshotToJson(int electricity, int salaries, int stationery, int cleaning)
        {
            var snapshot = new
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Electricity = electricity,
                Salaries = salaries,
                Stationery = stationery,
                Cleaning = cleaning
            };

            string path = "MonthlySnapshots.json";
            List<object> existing = File.Exists(path)
                ? JsonConvert.DeserializeObject<List<object>>(File.ReadAllText(path)) ?? new List<object>()
                : new List<object>();

            existing.Add(snapshot);
            File.WriteAllText(path, JsonConvert.SerializeObject(existing, Formatting.Indented));
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = "ExpenseReport.pdf" };
            if (save.ShowDialog() == DialogResult.OK)
            {
                Document doc = new Document();
                PdfWriter.GetInstance(doc, new FileStream(save.FileName, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                doc.Add(new Paragraph("School Expense Report", headerFont));
                doc.Add(new Paragraph("Date: " + DateTime.Now.ToString("dd MMMM yyyy")));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(lblElectricity.Text));
                doc.Add(new Paragraph(lblSalaries.Text));
                doc.Add(new Paragraph(lblStationery.Text));
                doc.Add(new Paragraph(lblCleaning.Text));
                doc.Add(new Paragraph(lblTotal.Text));

                doc.Close();
                MessageBox.Show("PDF exported successfully.");
            }
        }
    }

}