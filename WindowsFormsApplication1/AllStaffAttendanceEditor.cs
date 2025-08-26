using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class AllStaffAttendanceEditor : Form
    {
        private DataGridView dgv;
        private Dictionary<string, Dictionary<string, bool>> attendanceData;
        private List<string> staffIds;
        private List<string> allDates;

        public AllStaffAttendanceEditor()
        {
            InitializeComponent();
            LoadData();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "All Staff Attendance Editor";
            this.Size = new Size(1000, 600);

            dgv = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(940, 400),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            this.Controls.Add(dgv);

            Button markAllPresentBtn = new Button
            {
                Text = "✅ Mark All Present",
                Location = new Point(20, 20),
                Size = new Size(150, 30)
            };
            markAllPresentBtn.Click += MarkAllPresentBtn_Click;
            this.Controls.Add(markAllPresentBtn);

            Button saveBtn = new Button
            {
                Text = "💾 Save Locally",
                Location = new Point(190, 20),
                Size = new Size(120, 30)
            };
            saveBtn.Click += SaveBtn_Click;
            this.Controls.Add(saveBtn);

            Button uploadBtn = new Button
            {
                Text = "☁️ Upload to Firebase",
                Location = new Point(330, 20),
                Size = new Size(160, 30)
            };
            uploadBtn.Click += UploadBtn_Click;
            this.Controls.Add(uploadBtn);
        }

        private void LoadData()
        {
            attendanceData = new Dictionary<string, Dictionary<string, bool>>();
            staffIds = new List<string>();
            allDates = new List<string>();

            string path = "Attendance_Staff.json";
            if (!File.Exists(path))
            {
                MessageBox.Show("⚠️ No attendance file found.");
                return;
            }

            string json = File.ReadAllText(path);
            attendanceData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(json);

            // Collect all unique dates and staff IDs
            foreach (var dateEntry in attendanceData)
            {
                if (!allDates.Contains(dateEntry.Key))
                    allDates.Add(dateEntry.Key);

                foreach (var staff in dateEntry.Value)
                {
                    if (!staffIds.Contains(staff.Key))
                        staffIds.Add(staff.Key);
                }
            }

            allDates.Sort(); // Optional

            PopulateGrid();
        }

        private void PopulateGrid()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            dgv.Columns.Add("StaffID", "Staff ID");
            foreach (var date in allDates)
            {
                dgv.Columns.Add(date, DateTime.Parse(date).ToString("dd MMM"));
            }

            foreach (var staff in staffIds)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dgv);
                row.Cells[0].Value = staff;

                for (int i = 0; i < allDates.Count; i++)
                {
                    string date = allDates[i];
                    string status = "";

                    if (attendanceData.TryGetValue(date, out var staffDict))
                    {
                        if (staffDict.TryGetValue(staff, out bool present))
                        {
                            status = present ? "Present" : "Absent";
                        }
                    }

                    row.Cells[i + 1].Value = status;
                }

                dgv.Rows.Add(row);
            }
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0) return;

            var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string current = cell.Value?.ToString() ?? "";

            string newStatus = "";

            if (string.IsNullOrEmpty(current))
                newStatus = "Present";
            else if (current == "Present")
                newStatus = "Absent";
            else if (current == "Absent")
                newStatus = "";


            cell.Value = newStatus;
        }

        private void MarkAllPresentBtn_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                for (int i = 1; i < dgv.Columns.Count; i++)
                {
                    row.Cells[i].Value = "Present";
                }
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var newData = new Dictionary<string, Dictionary<string, bool>>();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string staffId = row.Cells[0].Value.ToString();

                for (int i = 1; i < dgv.Columns.Count; i++)
                {
                    string date = dgv.Columns[i].Name;
                    string value = row.Cells[i].Value?.ToString();

                    if (!newData.ContainsKey(date))
                        newData[date] = new Dictionary<string, bool>();

                    if (value == "Present")
                        newData[date][staffId] = true;
                    else if (value == "Absent")
                        newData[date][staffId] = false;
                    // Blank → don't add
                }
            }

            File.WriteAllText("Attendance_Staff.json", JsonConvert.SerializeObject(newData, Formatting.Indented));
            MessageBox.Show("✅ Saved locally!");
        }

        private async void UploadBtn_Click(object sender, EventArgs e)
        {
            string json = File.ReadAllText("Attendance_Staff.json");

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync("YOUR_API_KEY_HERE", content);

                    if (response.IsSuccessStatusCode)
                        MessageBox.Show("✅ Uploaded to Firebase!");
                    else
                        MessageBox.Show("❌ Upload failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error: " + ex.Message);
            }
        }
    }
}
