using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class FingerprintManager : Form
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, object>>> fingerprintsData =
            new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

        private Dictionary<string, Dictionary<string, Student>> studentsData =
            new Dictionary<string, Dictionary<string, Student>>();

        private SiticoneComboBox classSelector;
        private SiticoneComboBox newClassCombo;
        private SiticoneComboBox userSelector;
        private SiticoneButton moveBtn;
        private SiticoneDataGridView fingerprintGrid;
        private SiticoneTextBox searchBox;

        public FingerprintManager()
        {
            InitializeComponent();
            SetupUI();
            LoadStudentsJson();
            _ = LoadDataFromFirebase();
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;
            this.Size = new Size(1000, 700);
            this.Text = "🧠 Fingerprint Manager";

            var title = new Label
            {
                Text = "Fingerprint Manager",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 10),
                Size = new Size(500, 40)
            };
            this.Controls.Add(title);

            int yOffset = 60;

            AddLabel("Select Class:", 20, yOffset);
            classSelector = new SiticoneComboBox
            {
                Location = new Point(130, yOffset),
                Size = new Size(150, 35)
            };
            classSelector.Items.AddRange(new string[] { "P_G", "K_G", "NURSERY", "FIVE", "ONE", "TWO", "THREE", "FOUR", "Staff", "SIX", "SEVEN", "EIGHT", "NINETH", "TENTH" });
            classSelector.SelectedIndexChanged += ClassSelector_SelectedIndexChanged;
            this.Controls.Add(classSelector);

            AddLabel("Select User:", 320, yOffset);
            userSelector = new SiticoneComboBox
            {
                Location = new Point(420, yOffset),
                Size = new Size(150, 35)
            };
            this.Controls.Add(userSelector);

            AddLabel("Move To:", 600, yOffset);
            newClassCombo = new SiticoneComboBox
            {
                Location = new Point(680, yOffset),
                Size = new Size(150, 35)
            };
            newClassCombo.Items.AddRange(new string[] { "P_G", "K_G", "NURSERY", "FIVE", "ONE", "TWO", "THREE", "FOUR", "Staff", "SIX", "SEVEN", "EIGHT", "NINETH", "TENTH" });
            this.Controls.Add(newClassCombo);

            moveBtn = new SiticoneButton
            {
                Text = "Move ➡️",
                Location = new Point(850, yOffset),
                Size = new Size(100, 35),
                FillColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            moveBtn.Click += async (s, e) =>
            {
                if (classSelector.SelectedItem == null || userSelector.SelectedItem == null || newClassCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select all fields.");
                    return;
                }
                await MoveFingerprint(classSelector.SelectedItem.ToString(), userSelector.SelectedItem.ToString(), newClassCombo.SelectedItem.ToString());
            };
            this.Controls.Add(moveBtn);

            AddLabel("Search:", 20, 110);
            searchBox = new SiticoneTextBox
            {
                Location = new Point(90, 110),
                Size = new Size(200, 35),
                PlaceholderText = "Search..."
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            this.Controls.Add(searchBox);

            fingerprintGrid = new SiticoneDataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(940, 480),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            fingerprintGrid.Columns.Add("Class", "Class");
            fingerprintGrid.Columns.Add("ID", "ID");
            fingerprintGrid.Columns.Add("LogicID", "Logical ID");
            fingerprintGrid.Columns.Add("Name", "Name");

            var deleteCol = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Text = "🗑️ Delete",
                UseColumnTextForButtonValue = true
            };
            fingerprintGrid.Columns.Add(deleteCol);
            fingerprintGrid.CellClick += FingerprintGrid_CellClick;

            this.Controls.Add(fingerprintGrid);
        }

        private void AddLabel(string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, y + 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            this.Controls.Add(lbl);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string query = searchBox.Text.Trim().ToLower();
            foreach (DataGridViewRow row in fingerprintGrid.Rows)
            {
                bool match = false;
                for (int i = 0; i < 4; i++)
                {
                    string cellText = row.Cells[i].Value?.ToString().ToLower();
                    if (!string.IsNullOrEmpty(cellText) && cellText.Contains(query))
                    {
                        match = true;
                        break;
                    }
                }
                row.Visible = match;
            }
        }

        private void ClassSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleClassSelection();
        }

        private void HandleClassSelection()
        {
            userSelector.Items.Clear();
            string selectedClass = classSelector.SelectedItem?.ToString();
            if (selectedClass == null) return;

            if (fingerprintsData.ContainsKey(selectedClass))
            {
                foreach (var userKey in fingerprintsData[selectedClass].Keys)
                {
                    userSelector.Items.Add(userKey);
                }
            }

            _ = LoadFingerprintsIntoGrid(selectedClass);
        }


        private void LoadStudentsJson()
        {
            string file = "Students.json";
            if (System.IO.File.Exists(file))
            {
                var json = System.IO.File.ReadAllText(file);
                studentsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);
            }
        }

        private async Task LoadDataFromFirebase()
        {
            string url = "YOUR_API_KEY_HERE-Fingerprints.json";
            using (var client = new HttpClient())
            {
                var res = await client.GetStringAsync(url);
                fingerprintsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(res);
                PopulateGrid();
            }
        }

        private void PopulateGrid()
        {
            fingerprintGrid.Rows.Clear();

            foreach (var classEntry in fingerprintsData)
            {
                string className = classEntry.Key;
                int offset = GetClassOffsetForClass(className);

                foreach (var userEntry in classEntry.Value)
                {
                    string key = userEntry.Key;
                    int id = Convert.ToInt32(userEntry.Value["id"]);
                    int logicId = id - offset;
                    string logicalKey = key.StartsWith("Staff") ? key : $"Student{logicId}";
                    string name = "Unknown";

                    if (studentsData.ContainsKey(className) && studentsData[className].ContainsKey(logicalKey))
                        name = studentsData[className][logicalKey].Name;

                    fingerprintGrid.Rows.Add(className, id, logicId, name);
                }
            }
        }

        private Task LoadFingerprintsIntoGrid(string className)
        {
            fingerprintGrid.Rows.Clear();

            if (!fingerprintsData.ContainsKey(className)) return Task.CompletedTask;

            int offset = GetClassOffsetForClass(className);

            foreach (var userEntry in fingerprintsData[className])
            {
                string key = userEntry.Key;
                int id = Convert.ToInt32(userEntry.Value["id"]);
                int logicId = id - offset;
                string logicalKey = key.StartsWith("Staff") ? key : $"Student{logicId}";
                string name = "Unknown";

                if (studentsData.ContainsKey(className) && studentsData[className].ContainsKey(logicalKey))
                    name = studentsData[className][logicalKey].Name;

                fingerprintGrid.Rows.Add(className, id, logicId, name);
            }

            return Task.CompletedTask;
        }

        private async void FingerprintGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                string className = fingerprintGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
                int logicId = Convert.ToInt32(fingerprintGrid.Rows[e.RowIndex].Cells[2].Value);
                string key = className == "Staff" ? $"Staff{logicId}" : $"Student{logicId}";

                DialogResult result = MessageBox.Show("Are you sure you want to delete this fingerprint?", "Confirm", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    string url = $"YOUR_API_KEY_HERE{className}/{key}.json";
                    using (var client = new HttpClient())
                    {
                        await client.DeleteAsync(url);
                        MessageBox.Show("✅ Fingerprint deleted successfully.");
                        await LoadDataFromFirebase();
                    }
                }
            }
        }

        private int GetClassOffsetForClass(string className)
        {
            // Matches your ESP offsets
            switch (className)
            {
                case "ONE": return 0;
                case "TWO": return 65;
                case "THREE": return 130;
                case "FOUR": return 195;
                case "Staff": return 260;
                case "P_G": return 0;
                case "K_G": return 75;
                case "NURSERY": return 150;
                case "FIVE": return 225;
                case "SIX": return 0;
                case "SEVEN": return 60;
                case "EIGHT": return 120;
                case "NINETH": return 180;
                case "TENTH": return 240;
                default: return -1;
            }
        }

        private async Task MoveFingerprint(string currentClass, string currentUserKey, string newClass)
        {
            if (!fingerprintsData.ContainsKey(currentClass) || !fingerprintsData[currentClass].ContainsKey(currentUserKey))
            {
                MessageBox.Show("User not found.");
                return;
            }

            if (currentClass == newClass)
            {
                MessageBox.Show("Same class selected.");
                return;
            }

            var userData = fingerprintsData[currentClass][currentUserKey];
            int oldId = Convert.ToInt32(userData["id"]);
            int logicId = oldId - GetClassOffsetForClass(currentClass);
            int newId = GetClassOffsetForClass(newClass) + logicId;
            string newKey = currentUserKey.StartsWith("Staff") ? $"Staff{logicId}" : $"Student{logicId}";

            string checkUrl = $"YOUR_API_KEY_HERE{newClass}/{newKey}.json";
            using (var client = new HttpClient())
            {
                var existsRes = await client.GetAsync(checkUrl);
                if ((await existsRes.Content.ReadAsStringAsync()) != "null")
                {
                    MessageBox.Show("Same ID already exists in destination class.");
                    return;
                }
            }

            userData["id"] = newId;
            string newJson = JsonConvert.SerializeObject(userData);
            string uploadUrl = $"YOUR_API_KEY_HERE{newClass}/{newKey}.json";
            string deleteUrl = $"YOUR_API_KEY_HERE{currentClass}/{currentUserKey}.json";

            using (var client = new HttpClient())
            {
                var uploadRes = await client.PutAsync(uploadUrl, new StringContent(newJson, Encoding.UTF8, "application/json"));
                if (uploadRes.IsSuccessStatusCode)
                {
                    await client.DeleteAsync(deleteUrl);
                    MessageBox.Show("✅ Moved successfully.");
                    await LoadDataFromFirebase();
                }
                else
                {
                    MessageBox.Show("❌ Upload failed.");
                }
            }
        }
    }

   
}
