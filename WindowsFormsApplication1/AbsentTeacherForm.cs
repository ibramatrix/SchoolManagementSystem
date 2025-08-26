using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class AbsentTeacherForm : Form
    {
        private SiticoneComboBox gradeComboBox, periodComboBox, teacherComboBox, freeTeacherComboBox;
        private SiticoneButton assignBtn;
        private SiticoneHtmlLabel statusLabel;
        private Dictionary<string, SiticoneDataGridView> timetableGrids;
        private List<string> activeTeachers;

        public AbsentTeacherForm(List<string> teachers, Dictionary<string, Siticone.Desktop.UI.WinForms.SiticoneDataGridView> grids)
        {
            this.Text = "Replace Absent Teacher";
            this.Size = new Size(520, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.activeTeachers = teachers;
            this.timetableGrids = grids;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;

            // Labels + ComboBoxes arranged vertically with spacing
            int leftMarginLabel = 30;
            int leftMarginCombo = 170;
            int topStart = 30;
            int verticalSpacing = 50;
            int comboWidth = 300;
            int comboHeight = 36;

            var fontLabel = new Font("Segoe UI", 10, FontStyle.Regular);

            // Grade
            var lblGrade = new SiticoneHtmlLabel
            {
                Text = "Select Grade:",
                Location = new Point(leftMarginLabel, topStart + 5),
                Font = fontLabel,
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true
            };
            this.Controls.Add(lblGrade);

            gradeComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMarginCombo, topStart),
                Size = new Size(comboWidth, comboHeight),
                FillColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gradeComboBox.Items.AddRange(timetableGrids.Keys.ToArray());
            gradeComboBox.SelectedIndexChanged += RefreshAbsentTeachers;
            this.Controls.Add(gradeComboBox);

            // Period
            var lblPeriod = new SiticoneHtmlLabel
            {
                Text = "Select Period:",
                Location = new Point(leftMarginLabel, topStart + verticalSpacing + 5),
                Font = fontLabel,
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true
            };
            this.Controls.Add(lblPeriod);

            periodComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMarginCombo, topStart + verticalSpacing),
                Size = new Size(comboWidth, comboHeight),
                FillColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int i = 1; i <= 8; i++) periodComboBox.Items.Add(i.ToString());
            periodComboBox.SelectedIndexChanged += RefreshAbsentTeachers;
            this.Controls.Add(periodComboBox);

            // Absent Teacher
            var lblAbsentTeacher = new SiticoneHtmlLabel
            {
                Text = "Absent Teacher:",
                Location = new Point(leftMarginLabel, topStart + verticalSpacing * 2 + 5),
                Font = fontLabel,
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true
            };
            this.Controls.Add(lblAbsentTeacher);

            teacherComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMarginCombo, topStart + verticalSpacing * 2),
                Size = new Size(comboWidth, comboHeight),
                FillColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false // just for showing the absent teacher, no selection
            };
            this.Controls.Add(teacherComboBox);

            // Available Teachers
            var lblFreeTeacher = new SiticoneHtmlLabel
            {
                Text = "Available Teachers:",
                Location = new Point(leftMarginLabel, topStart + verticalSpacing * 3 + 5),
                Font = fontLabel,
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true
            };
            this.Controls.Add(lblFreeTeacher);

            freeTeacherComboBox = new SiticoneComboBox()
            {
                Location = new Point(leftMarginCombo, topStart + verticalSpacing * 3),
                Size = new Size(comboWidth, comboHeight),
                FillColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(freeTeacherComboBox);

            // Assign Button
            assignBtn = new SiticoneButton()
            {
                Text = "Assign Alternate",
                Location = new Point(leftMarginCombo, topStart + verticalSpacing * 4 + 5),
                Size = new Size(comboWidth, 40),
                FillColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BorderRadius = 8
            };
            assignBtn.Click += AssignBtn_Click;
            this.Controls.Add(assignBtn);

            // Status Label
            statusLabel = new SiticoneHtmlLabel()
            {
                Text = "",
                Location = new Point(leftMarginLabel, topStart + verticalSpacing * 5 + 15),
                Size = new Size(450, 30),
                ForeColor = Color.SeaGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Italic)
            };
            this.Controls.Add(statusLabel);
        }

        private void RefreshAbsentTeachers(object sender, EventArgs e)
        {
            teacherComboBox.Items.Clear();
            freeTeacherComboBox.Items.Clear();
            teacherComboBox.Text = "";
            freeTeacherComboBox.Text = "";
            statusLabel.Text = "";

            if (gradeComboBox.SelectedItem == null || periodComboBox.SelectedItem == null)
                return;

            string grade = gradeComboBox.SelectedItem.ToString();
            int period = int.Parse(periodComboBox.SelectedItem.ToString()) - 1;

            if (!timetableGrids.ContainsKey(grade)) return;

            var grid = timetableGrids[grade];
            if (grid.RowCount < 2 && grade != "P_G" && grade != "K_G" && grade != "NURSERY")
                return;

            string currentTeacher = null;
            if (grade == "P_G" || grade == "K_G" || grade == "NURSERY")
            {
                // Pre-grade classes have one row for teacher
                currentTeacher = grid.Rows[0].Cells[period].Value?.ToString();
            }
            else
            {
                currentTeacher = grid.Rows[1].Cells[period].Value?.ToString();
            }

            if (!string.IsNullOrWhiteSpace(currentTeacher))
                teacherComboBox.Items.Add(currentTeacher);

            if (teacherComboBox.Items.Count > 0)
                teacherComboBox.SelectedIndex = 0;

            // Find busy teachers for this period
            var busyTeachers = new HashSet<string>();
            foreach (var g in timetableGrids)
            {
                var gv = g.Value;
                int rowIdx = (gv.RowCount == 1) ? 0 : 1;
                string t = gv.Rows[rowIdx].Cells[period].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(t))
                    busyTeachers.Add(t);
            }

            var free = activeTeachers.Where(t => !busyTeachers.Contains(t)).ToList();
            foreach (var t in free)
                freeTeacherComboBox.Items.Add(t);

            if (freeTeacherComboBox.Items.Count > 0)
                freeTeacherComboBox.SelectedIndex = 0;
        }

        private void AssignBtn_Click(object sender, EventArgs e)
        {
            if (gradeComboBox.SelectedItem == null || periodComboBox.SelectedItem == null || freeTeacherComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select all fields.", "Missing Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string grade = gradeComboBox.SelectedItem.ToString();
            int period = int.Parse(periodComboBox.SelectedItem.ToString()) - 1;
            string newTeacher = freeTeacherComboBox.SelectedItem.ToString();

            var grid = timetableGrids[grade];
            int rowIndex = (grid.RowCount == 1) ? 0 : 1; // For class teacher vs subject teacher

            grid.Rows[rowIndex].Cells[period].Value = newTeacher;
            statusLabel.Text = $"Replaced successfully with: {newTeacher}";
        }
    }
}
