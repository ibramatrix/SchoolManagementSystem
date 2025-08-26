using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class AttendanceViewer : Form
    {
        private ComboBox classComboBox;
        private ComboBox monthComboBox;
        private DataGridView dgv;
        private Button exportPdfButton;
        private Button saveButton;
        private Button markAllPresentButton;

        private Dictionary<string, Dictionary<string, Student>> allClasses;
        private FirebaseClient firebase;

        private Dictionary<string, Dictionary<string, Dictionary<string, bool>>> onlineAttendance =
            new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();

        private Dictionary<Tuple<string, string>, bool> manualEdits =
            new Dictionary<Tuple<string, string>, bool>(); // ✅ C# 7.3 compatible

        List<string> yearMonthList;

        public AttendanceViewer()
        {
            InitializeComponent();
            InitializeUI();
            LoadClassData();
        }

        private void InitializeFirebase(string selectedClass)
        {
            firebase = new FirebaseClient(
                "YOUR_API_KEY_HERE",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult("YOUR_API_KEY_HERE")
                });

            firebase
                .Child("Attendance")
                .Child(selectedClass)
                .AsObservable<Dictionary<string, bool>>()
                .Subscribe(fb =>
                {
                    if (fb.Object == null) return;
                    string dateKey = fb.Key;

                    if (!onlineAttendance.ContainsKey(selectedClass))
                        onlineAttendance[selectedClass] = new Dictionary<string, Dictionary<string, bool>>();

                    onlineAttendance[selectedClass][dateKey] = fb.Object;

                    BeginInvoke((Action)LoadAttendance);
                });
        }

        private void InitializeUI()
        {
            this.Text = "📊 Attendance Viewer";
            this.Size = new Size(1000, 600);
            this.BackColor = Color.White;

            classComboBox = new ComboBox { Location = new Point(20, 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            classComboBox.SelectedIndexChanged += async (s, e) =>
            {
                string selectedClass = classComboBox.SelectedItem.ToString();
                InitializeFirebase(selectedClass);
                await FetchInitialAttendance(selectedClass);
            };

            monthComboBox = new ComboBox { Location = new Point(220, 20), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            yearMonthList = Enumerable
                .Range(DateTime.Now.Year - 1, 5)
                .SelectMany(y => Enumerable.Range(1, 12).Select(m => new DateTime(y, m, 1)))
                .Select(dt => dt.ToString("MMMM yyyy"))
                .ToList();
            monthComboBox.Items.AddRange(yearMonthList.ToArray());
            monthComboBox.SelectedItem = DateTime.Now.ToString("MMMM yyyy");
            monthComboBox.SelectedIndexChanged += (s, e) => LoadStudentAndAttendance();

            exportPdfButton = new Button { Text = "🖨️ Export PDF", Location = new Point(800, 20), Width = 100 };
            exportPdfButton.Click += ExportPdfButton_Click;

            saveButton = new Button { Text = "💾 Save Attendance", Location = new Point(680, 20), Width = 110 };
            saveButton.Click += async (s, e) => await SaveManualAttendance();

            markAllPresentButton = new Button { Text = "✔ Mark All Present", Location = new Point(500, 20), Width = 150 };
            markAllPresentButton.Click += MarkAllPresent;

            dgv = new DataGridView
            {
                Location = new Point(20, 60),
                Width = 940,
                Height = 470,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                SelectionMode = DataGridViewSelectionMode.CellSelect
            };
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            this.Controls.AddRange(new Control[] {
                classComboBox, monthComboBox, markAllPresentButton,
                saveButton, exportPdfButton, dgv
            });
        }

        private async Task FetchInitialAttendance(string selectedClass)
        {
            try
            {
                var classAttendance = await firebase
                    .Child("Attendance")
                    .Child(selectedClass)
                    .OnceAsync<Dictionary<string, bool>>();

                if (!onlineAttendance.ContainsKey(selectedClass))
                    onlineAttendance[selectedClass] = new Dictionary<string, Dictionary<string, bool>>();

                foreach (var day in classAttendance)
                    onlineAttendance[selectedClass][day.Key] = day.Object;

                BeginInvoke((Action)LoadAttendance);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading attendance: " + ex.Message);
            }
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 2) return;

            string studentId = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
            string dateKey = dgv.Columns[e.ColumnIndex].Name;

            var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
            bool isPresent = cell.Value?.ToString() == "✔";

            cell.Value = isPresent ? "❌" : "✔";
            manualEdits[new Tuple<string, string>(studentId, dateKey)] = !isPresent;
        }

        private void MarkAllPresent(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string studentId = row.Cells[0].Value.ToString();
                for (int i = 2; i < dgv.Columns.Count; i++)
                {
                    string dateKey = dgv.Columns[i].Name;
                    row.Cells[i].Value = "✔";
                    manualEdits[new Tuple<string, string>(studentId, dateKey)] = true;
                }
            }
        }

        private async Task SaveManualAttendance()
        {
            string selectedClass = classComboBox.SelectedItem?.ToString();
            if (selectedClass == null)
            {
                MessageBox.Show("⚠ Please select a class first.");
                return;
            }

            foreach (var kvp in manualEdits)
            {
                string studentId = kvp.Key.Item1;
                string dateKey = kvp.Key.Item2;
                bool present = kvp.Value;

                await firebase
                  .Child("Attendance")
                  .Child(selectedClass)
                  .Child(dateKey)
                  .Child(studentId)
                  .PutAsync(present);

                string filename = $"Attendance_{selectedClass}.json";
                var allOffline = File.Exists(filename)
                    ? JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(filename))
                    : new Dictionary<string, Dictionary<string, bool>>();

                if (!allOffline.ContainsKey(dateKey))
                    allOffline[dateKey] = new Dictionary<string, bool>();

                allOffline[dateKey][studentId] = present;
                File.WriteAllText(filename, JsonConvert.SerializeObject(allOffline, Formatting.Indented));
            }

            MessageBox.Show("✅ Manual attendance saved to Firebase.");
            manualEdits.Clear();
        }

        private void LoadClassData()
        {
            string filePath = "Students.json";
            if (!File.Exists(filePath)) return;

            string json = File.ReadAllText(filePath);
            allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);
            classComboBox.Items.Clear();
            classComboBox.Items.AddRange(allClasses.Keys.ToArray());
            if (classComboBox.Items.Count > 0) classComboBox.SelectedIndex = 0;
        }

        private void LoadStudentAndAttendance()
        {
            LoadAttendance();
        }

        private void LoadAttendance()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            if (classComboBox.SelectedItem == null || monthComboBox.SelectedItem == null)
                return;

            string selectedClass = classComboBox.SelectedItem.ToString();
            string monthYear = monthComboBox.SelectedItem.ToString();
            DateTime selectedDate = DateTime.ParseExact(monthYear, "MMMM yyyy", CultureInfo.InvariantCulture);
            string attendancePath = $"Attendance_{selectedClass}.json";

            var allOffline = File.Exists(attendancePath)
                ? JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(attendancePath))
                : new Dictionary<string, Dictionary<string, bool>>();

            var combined = new Dictionary<string, Dictionary<string, bool>>(allOffline);

            if (onlineAttendance.ContainsKey(selectedClass))
            {
                foreach (var dateEntry in onlineAttendance[selectedClass])
                {
                    if (!combined.ContainsKey(dateEntry.Key))
                        combined[dateEntry.Key] = new Dictionary<string, bool>();

                    foreach (var sid in dateEntry.Value)
                        combined[dateEntry.Key][sid.Key] = sid.Value;
                }
            }

            var students = allClasses[selectedClass];

            dgv.Columns.Add("StudentId", "ID");
            dgv.Columns.Add("StudentName", "Student Name");

            int daysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(selectedDate.Year, selectedDate.Month, day);
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;
                dgv.Columns.Add(date.ToString("yyyy-MM-dd"), day.ToString());
            }

            foreach (var kvp in students)
            {
                string studentId = kvp.Key;
                string studentName = kvp.Value.Name;
                List<object> row = new List<object> { studentId, studentName };

                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime date = new DateTime(selectedDate.Year, selectedDate.Month, day);
                    if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                    string dateKey = date.ToString("yyyy-MM-dd");
                    string status = "—";

                    if (combined.ContainsKey(dateKey) && combined[dateKey].ContainsKey(studentId))
                        status = combined[dateKey][studentId] ? "✔" : "❌";

                    row.Add(status);
                }

                dgv.Rows.Add(row.ToArray());
            }
        }

        private void ExportPdfButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("🖨️ PDF export functionality will be implemented soon.");
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
