using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class TimetableManagerForm : UserControl
    {
        private readonly List<string> gradeLevels = new List<string> {
            "P_G", "K_G", "NURSERY", "ONE", "TWO", "THREE",
            "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN"
        };

        private Dictionary<string, SiticoneDataGridView> timetableGrids = new Dictionary<string, SiticoneDataGridView>();
        private List<string> activeTeachers = new List<string>();

        private SiticoneTabControl tabControl;
        private SiticoneButton absentBtn;
        private SiticoneButton saveBtn;
        private SiticoneHtmlLabel titleLabel;

        public TimetableManagerForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadStaff();
            LoadTimetable();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Title label docked top
            titleLabel = new SiticoneHtmlLabel()
            {
                Text = "<b>Timetable Manager</b>",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlignment = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            // TabControl dock fill (adjusted height for buttons)
            tabControl = new SiticoneTabControl()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                ItemSize = new Size(150, 40),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(0, 0),  // No internal padding
                Margin = new Padding(0),
                TabButtonHoverState =
                {
                    FillColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White
                },
                TabButtonSelectedState =
                {
                    FillColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White
                }
            };
            this.Controls.Add(tabControl);

            // Create tabs and grids
            foreach (var grade in gradeLevels)
            {
                var tabPage = new TabPage(grade)
                {
                    BackColor = Color.White,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };

                var dgv = new SiticoneDataGridView
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    ColumnCount = 8,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.WhiteSmoke,
                    BorderStyle = BorderStyle.None,
                    GridColor = Color.Gainsboro,
                    RowHeadersWidth = 60,
                    ColumnHeadersHeight = 35,
                    EnableHeadersVisualStyles = false,
                    SelectionMode = DataGridViewSelectionMode.CellSelect,
                    MultiSelect = false,
                    Font = new Font("Segoe UI", 10),
                    RowTemplate = { Height = 28 }
                };

                // Style column headers
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Style row headers
                dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
                dgv.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10);
                dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                for (int i = 0; i < 8; i++)
                {
                    dgv.Columns[i].HeaderText = "Period " + (i + 1);
                    dgv.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                if (gradeLevels.IndexOf(grade) <= 5) // P_G to THREE
                {
                    dgv.RowCount = 1;
                    dgv.Rows[0].HeaderCell.Value = "Class Teacher";
                }
                else
                {
                    dgv.RowCount = 2;
                    dgv.Rows[0].HeaderCell.Value = "Subjects";
                    dgv.Rows[1].HeaderCell.Value = "Teachers";
                }

                timetableGrids[grade] = dgv;
                tabPage.Controls.Add(dgv);
                tabControl.TabPages.Add(tabPage);
            }

            // Panel for buttons at bottom right
            var buttonPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10),
                BackColor = Color.White
            };
            this.Controls.Add(buttonPanel);

            // Save Button
            saveBtn = new SiticoneButton()
            {
                Text = "Save Timetable",
                Size = new Size(140, 40),
                FillColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BorderRadius = 8,
                Anchor = AnchorStyles.Right
            };
            saveBtn.Click += SaveBtn_Click;
            buttonPanel.Controls.Add(saveBtn);

            // Absent Button
            absentBtn = new SiticoneButton()
            {
                Text = "Mark Teacher Absent / Replace",
                Size = new Size(240, 40),
                FillColor = Color.OrangeRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BorderRadius = 8,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(10, 0, 0, 0)
            };
            absentBtn.Click += AbsentBtn_Click;
            buttonPanel.Controls.Add(absentBtn);

            // Layout buttons right-aligned with spacing
            buttonPanel.Resize += (s, e) =>
            {
                int spacing = 10;
                int x = buttonPanel.Width - absentBtn.Width - spacing;

                absentBtn.Location = new Point(x, (buttonPanel.Height - absentBtn.Height) / 2);
                x -= (saveBtn.Width + spacing);
                saveBtn.Location = new Point(x, (buttonPanel.Height - saveBtn.Height) / 2);
            };
        }

        private void AbsentBtn_Click(object sender, EventArgs e)
        {
            var popup = new AbsentTeacherForm(activeTeachers, timetableGrids);
            popup.ShowDialog();
        }

        private void LoadStaff()
        {
            activeTeachers.Clear();

            try
            {
                if (!File.Exists("staff.json"))
                {
                    MessageBox.Show("staff.json not found.");
                    return;
                }

                var json = File.ReadAllText("staff.json");
                var staffData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                foreach (var entry in staffData)
                {
                    if (entry.Value.TryGetValue("Name", out var nameObj))
                    {
                        string name = nameObj?.ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                            activeTeachers.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff.json: " + ex.Message);
            }
        }

        private void LoadTimetable()
        {
            if (!File.Exists("timetable.json"))
                return;

            try
            {
                var json = File.ReadAllText("timetable.json");
                var data = JsonConvert.DeserializeObject<Dictionary<string, List<List<string>>>>(json);

                foreach (var entry in data)
                {
                    var grade = entry.Key;
                    if (!timetableGrids.ContainsKey(grade))
                        continue;

                    var dgv = timetableGrids[grade];
                    var rows = entry.Value;

                    int rowCount = Math.Min(rows.Count, dgv.RowCount);

                    for (int i = 0; i < rowCount; i++)
                    {
                        int colCount = Math.Min(rows[i].Count, dgv.ColumnCount);
                        for (int j = 0; j < colCount; j++)
                        {
                            dgv.Rows[i].Cells[j].Value = rows[i][j];
                        }
                    }
                }

                ApplyTeacherDropdowns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading timetable.json: " + ex.Message);
            }
        }

        private void ApplyTeacherDropdowns()
        {
            int periodCount = 8;

            for (int period = 0; period < periodCount; period++)
            {
                HashSet<string> busyTeachers = new HashSet<string>();

                foreach (var grid in timetableGrids.Values)
                {
                    int row = (grid.RowCount == 1) ? 0 : 1;
                    string teacher = grid.Rows[row].Cells[period].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(teacher))
                        busyTeachers.Add(teacher);
                }

                List<string> freeTeachers = activeTeachers.Where(t => !busyTeachers.Contains(t)).ToList();

                foreach (var grid in timetableGrids.Values)
                {
                    int teacherRowIndex = (grid.RowCount == 1) ? 0 : 1;

                    var comboBoxCell = new DataGridViewComboBoxCell();

                    string currentValue = grid.Rows[teacherRowIndex].Cells[period].Value?.ToString();

                    var comboItems = new List<string>(freeTeachers);

                    if (!string.IsNullOrWhiteSpace(currentValue) && !comboItems.Contains(currentValue))
                        comboItems.Add(currentValue);

                    comboBoxCell.DataSource = comboItems;
                    comboBoxCell.FlatStyle = FlatStyle.Flat;
                    comboBoxCell.Value = currentValue ?? "";

                    grid.Rows[teacherRowIndex].Cells[period] = comboBoxCell;
                }
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var result = new Dictionary<string, List<List<string>>>();

                foreach (var entry in timetableGrids)
                {
                    var dgv = entry.Value;
                    var gridData = new List<List<string>>();

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        var rowData = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            rowData.Add(cell.Value?.ToString() ?? "");
                        }
                        gridData.Add(rowData);
                    }

                    result[entry.Key] = gridData;
                }

                var jsonOut = JsonConvert.SerializeObject(result, Formatting.Indented);
                File.WriteAllText("timetable.json", jsonOut);

                MessageBox.Show("Timetable saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving timetable: " + ex.Message);
            }
        }
    }
}
