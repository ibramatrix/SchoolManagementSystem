using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class StudentMainPrin : UserControl
    {
        private Dictionary<string, Dictionary<string, Student>> allClasses =
            new Dictionary<string, Dictionary<string, Student>>();

        private SiticoneComboBox classFilterDropdown;
        private SiticoneTextBox searchBox;
        private SiticoneDataGridView dgv;
        private SiticoneVScrollBar siticoneVScrollBar;


        private static readonly string firebaseBaseUrl = "YOUR_API_KEY_HERE";
        private static readonly string firebaseAuth = "YOUR_API_KEY_HERE";


        public StudentMainPrin()
        {
            InitializeComponent();
            SetupUI();
            _ = LoadStudentsFromFirebase();
        }

        private async Task UpdateStudentInFirebase(string className, string studentId, Student student)
        {
            using (var client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(student);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{firebaseBaseUrl}/{className}/{studentId}.json{firebaseAuth}", content);
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task DeleteStudentFromFirebase(string className, string studentId)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{firebaseBaseUrl}/{className}/{studentId}.json{firebaseAuth}");
                response.EnsureSuccessStatusCode();
            }
        }

        private void SetupUI()
        {
            this.Text = "Student Records";
            this.Font = new Font("Segoe UI", 10);

            // Combo box for class filter
            classFilterDropdown = new SiticoneComboBox
            {
                Width = 200,
                Location = new Point(20, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            classFilterDropdown.SelectedIndexChanged += ClassFilterDropdown_SelectedIndexChanged;
            this.Controls.Add(classFilterDropdown);

            // Search box
            searchBox = new SiticoneTextBox
            {
                PlaceholderText = "Search by name...",
                Width = 200,
                Location = new Point(240, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            this.Controls.Add(searchBox);

            // DataGridView
            dgv = new SiticoneDataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.EnableHeadersVisualStyles = false;

            // Columns
            dgv.Columns.Add("AdmissionNumber", "ID");
            dgv.Columns.Add("Name", "Name");
            dgv.Columns.Add("FatherName", "Father Name");
            dgv.Columns.Add("Address", "Address");
            dgv.Columns.Add("Contact", "Contact");
            dgv.Columns.Add("Fee", "Fee");
            dgv.Columns.Add("Class", "Class");

            dgv.Columns["Class"].Width = 70;
            dgv.Columns["Class"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgv.Columns["Fee"].Width = 70;
            dgv.Columns["Fee"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;


            // Hide the student ID column
            dgv.Columns["AdmissionNumber"].Visible = false;

            // Actions column
            var actionsCol = new DataGridViewImageColumn
            {
                Name = "Actions",
                HeaderText = "Actions",
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };

            
            dgv.Columns.Add(actionsCol);
            dgv.Columns["Actions"].Width = 170;
            dgv.Columns["Actions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgv.CellMouseClick += Dgv_CellMouseClick;
            dgv.CellPainting += Dgv_CellPainting;

            dgv.ScrollBars = ScrollBars.None;

            // Increase row height
            dgv.RowTemplate.Height = 40;

            // Create Siticone vertical scrollbar
            siticoneVScrollBar = new Siticone.Desktop.UI.WinForms.SiticoneVScrollBar
            {
                Location = new Point(dgv.Right - 15, dgv.Top),
                Height = dgv.Height,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                LargeChange = dgv.DisplayedRowCount(false), // number of visible rows
                Minimum = 0,
                Maximum = dgv.RowCount > 0 ? dgv.RowCount - 1 : 0,
                SmallChange = 1,
                Value = 0,
                Visible = true,
                ScrollbarSize = 15,
                FillColor = Color.FromArgb(245, 245, 245),
                ThumbColor = Color.FromArgb(41, 128, 185) // green thumb color
            };

            // Add scrollbar to Controls
            this.Controls.Add(siticoneVScrollBar);

            // Sync scrollbar max and value with dgv row count and scrolling
            dgv.RowsAdded += (s, e) =>
            {
                siticoneVScrollBar.Maximum = dgv.RowCount > 0 ? dgv.RowCount - 1 : 0;
            };

            dgv.RowsRemoved += (s, e) =>
            {
                siticoneVScrollBar.Maximum = dgv.RowCount > 0 ? dgv.RowCount - 1 : 0;
            };

            siticoneVScrollBar.Scroll += (s, e) =>
            {
                if (dgv.RowCount == 0) return;
                int maxFirstDisplayed = dgv.RowCount - dgv.DisplayedRowCount(false);
                int val = Math.Min(siticoneVScrollBar.Value, maxFirstDisplayed);
                dgv.FirstDisplayedScrollingRowIndex = val;
            };

            // Also update scrollbar height and position on resize
            this.Resize += (s, e) =>
            {
                siticoneVScrollBar.Location = new Point(dgv.Right - siticoneVScrollBar.Width, dgv.Top);
                siticoneVScrollBar.Height = dgv.Height;
            };

            this.Controls.Add(dgv);
        }

        private async Task LoadStudentsFromFirebase()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{firebaseBaseUrl}.json{firebaseAuth}");
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(json) && json != "null")
                    {
                        // Your data is structured as: { "ClassName": { "StudentId": { studentObject } } }
                        allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);

                        classFilterDropdown.Items.Clear();

                        foreach (var classEntry in allClasses)
                        {
                            classFilterDropdown.Items.Add(classEntry.Key);
                        }

                        // Default to P_G if available, otherwise load first class
                        if (allClasses.ContainsKey("P_G"))
                        {
                            classFilterDropdown.SelectedItem = "P_G";
                            DisplayStudents("P_G");
                        }
                        else if (classFilterDropdown.Items.Count > 0)
                        {
                            classFilterDropdown.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        MessageBox.Show("No students found in Firebase.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading students from Firebase: {ex.Message}");
                }
            }
        }




        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.PaintBackground(e.CellBounds, true);

                int iconSize = 28;
                int padding = 6;

                Image attendanceIcon = Properties.Resources.verification;
                Image editIcon = Properties.Resources.edit;
                Image deleteIcon = Properties.Resources.delete;
                Image reportIcon = Properties.Resources.report; // <-- your report icon resource

                int attendanceX = e.CellBounds.Left + padding;
                int editX = attendanceX + iconSize + padding;
                int deleteX = editX + iconSize + padding;
                int reportX = deleteX + iconSize + padding;  // 4th icon position

                int iconY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                using (Brush bgBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                {
                    int bgWidth = (iconSize * 4) + (padding * 5);  // adjust width for 4 icons
                    Rectangle bgRect = new Rectangle(e.CellBounds.Left + padding / 2, e.CellBounds.Top + padding / 2, bgWidth, e.CellBounds.Height - padding);
                    e.Graphics.FillRectangle(bgBrush, bgRect);
                }

                e.Graphics.DrawImage(attendanceIcon, new Rectangle(attendanceX, iconY, iconSize, iconSize));
                e.Graphics.DrawImage(editIcon, new Rectangle(editX, iconY, iconSize, iconSize));
                e.Graphics.DrawImage(deleteIcon, new Rectangle(deleteX, iconY, iconSize, iconSize));
                e.Graphics.DrawImage(reportIcon, new Rectangle(reportX, iconY, iconSize, iconSize));  // Draw report icon

                e.Handled = true;
            }
        }



        private void ClassFilterDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedClass = classFilterDropdown.SelectedItem.ToString();
            DisplayStudents(selectedClass);
        }

        private void DisplayStudents(string className)
        {
            dgv.Rows.Clear();
            if (allClasses.ContainsKey(className))
            {
                foreach (var studentEntry in allClasses[className])
                {
                    Student s = studentEntry.Value;
                    dgv.Rows.Add(studentEntry.Key, s.Name, s.FatherName, s.Address, s.Contact, s.Fee, className, null);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string selectedClass = classFilterDropdown.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClass)) return;

            string searchTerm = searchBox.Text.ToLower();
            dgv.Rows.Clear();

            foreach (var studentEntry in allClasses[selectedClass])
            {
                Student s = studentEntry.Value;
                if (s.Name.ToLower().Contains(searchTerm))
                {
                    dgv.Rows.Add(studentEntry.Key, s.Name, s.FatherName, s.Address, s.Contact, s.Fee, selectedClass, null);
                }
            }
        }

        private async void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].Name == "Actions")
            {
                int iconSize = 28; // same size as painting
                int padding = 6;
                var cellRect = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

                int attendanceX = padding;
                int editX = attendanceX + iconSize + padding;
                int deleteX = editX + iconSize + padding;
                int reportX = deleteX + iconSize + padding;

                int clickX = e.X;

                string studentId = dgv.Rows[e.RowIndex].Cells["AdmissionNumber"].Value.ToString();
                string className = dgv.Rows[e.RowIndex].Cells["Class"].Value.ToString();
                string studentName = dgv.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                string studentContact = dgv.Rows[e.RowIndex].Cells["Contact"].Value.ToString();

                if (clickX >= attendanceX && clickX < attendanceX + iconSize)
                {
                    var viewer = new StudentAttendanceViewer(studentId, className, studentName, studentContact);
                    viewer.Show();
                }
                else if (clickX >= editX && clickX < editX + iconSize)
                {
                    // Get the student object from your dictionary
                    if (allClasses.TryGetValue(className, out var students) && students.TryGetValue(studentId, out var student))
                    {
                        using (var editForm = new EditStudentForm(student))
                        {
                            var result = editForm.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                // Get updated student from the form
                                Student updatedStudent = editForm.UpdatedStudent;

                                // Update dictionary
                                students[studentId] = updatedStudent;

                                // Update grid row to reflect changes
                                dgv.Rows[e.RowIndex].Cells["Name"].Value = updatedStudent.Name;
                                dgv.Rows[e.RowIndex].Cells["FatherName"].Value = updatedStudent.FatherName;
                                dgv.Rows[e.RowIndex].Cells["Address"].Value = updatedStudent.Address;
                                dgv.Rows[e.RowIndex].Cells["Contact"].Value = updatedStudent.Contact;
                                dgv.Rows[e.RowIndex].Cells["Fee"].Value = updatedStudent.Fee;


                                await UpdateStudentInFirebase(className, studentId, updatedStudent);
                            }
                        }
                    }
                }

                else if (clickX >= deleteX && clickX < deleteX + iconSize)
                {
                    if (MessageBox.Show("Are you sure to delete this student?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        allClasses[className].Remove(studentId);
                        dgv.Rows.RemoveAt(e.RowIndex);
                        await DeleteStudentFromFirebase(className, studentId); // Remove from Firebase

                    }
                }

                else if (clickX >= reportX && clickX < reportX + iconSize)
                {
                    // Open the TestReports form
                    var reportForm = new TestReports(studentId, className, studentName);
                    reportForm.ShowDialog();
                }
            }
        }

     

    }
}
