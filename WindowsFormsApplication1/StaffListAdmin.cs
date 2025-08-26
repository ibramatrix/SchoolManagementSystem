using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class StaffListAdmin : UserControl
    {
        private SiticoneDataGridView dgv;
        private Dictionary<string, Staff> staffData = new Dictionary<string, Staff>();
        private SiticoneVScrollBar vScrollBar;
        private SiticoneTextBox searchBox;

        private const int IconSize = 28;
        private const int IconSpacing = 10;

        public StaffListAdmin()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadStaffFromFirebaseAsync(); // load staff from Firebase
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            // Top panel for search box
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };

            searchBox = new SiticoneTextBox
            {
                PlaceholderText = "Search staff...",
                Width = 200,
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 10),
                BorderRadius = 0,
                FillColor = Color.White,
                BorderColor = Color.LightGray,
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchPanel.Controls.Add(searchBox);

            dgv = new SiticoneDataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.None,
                RowTemplate = { Height = 40 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            vScrollBar = new SiticoneVScrollBar
            {
                Location = new Point(dgv.Right - 15, dgv.Top),
                Height = dgv.Height,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                LargeChange = dgv.DisplayedRowCount(false),
                Minimum = 0,
                Maximum = dgv.RowCount > 0 ? dgv.RowCount - 1 : 0,
                SmallChange = 1,
                Value = 0,
                Visible = true,
                ScrollbarSize = 15,
                FillColor = Color.FromArgb(245, 245, 245),
                ThumbColor = Color.FromArgb(41, 128, 185)
            };

            dgv.RowsAdded += (s, e) => UpdateScrollBar();
            dgv.RowsRemoved += (s, e) => UpdateScrollBar();

            vScrollBar.Scroll += (s, e) =>
            {
                if (dgv.RowCount == 0) return;
                int maxFirstDisplayed = dgv.RowCount - dgv.DisplayedRowCount(false);
                dgv.FirstDisplayedScrollingRowIndex = Math.Min(vScrollBar.Value, maxFirstDisplayed);
            };

            dgv.Scroll += (s, e) =>
            {
                if (dgv.RowCount == 0) return;
                int maxFirstDisplayed = dgv.RowCount - dgv.DisplayedRowCount(false);
                int firstDisplayed = dgv.FirstDisplayedScrollingRowIndex;
                if (firstDisplayed >= 0 && firstDisplayed <= maxFirstDisplayed)
                {
                    vScrollBar.Value = firstDisplayed;
                }
            };

            this.Resize += (s, e) =>
            {
                dgv.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 80);
                vScrollBar.Location = new Point(dgv.Right - vScrollBar.ScrollbarSize, dgv.Top);
                vScrollBar.Height = dgv.Height;
            };

            this.Controls.Add(searchPanel);
            this.Controls.Add(dgv);
            this.Controls.Add(vScrollBar);
            vScrollBar.BringToFront();

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;
        }

        private void UpdateScrollBar()
        {
            vScrollBar.Maximum = dgv.RowCount > 0 ? dgv.RowCount - 1 : 0;
            vScrollBar.LargeChange = dgv.DisplayedRowCount(false);
        }

        private async Task LoadStaffFromFirebaseAsync()
        {
            try
            {
                string baseUrl = "YOUR_API_KEY_HERE";
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(baseUrl);
                    if (string.IsNullOrEmpty(response) || response == "null")
                    {
                        staffData = new Dictionary<string, Staff>();
                        return;
                    }

                    staffData = JsonConvert.DeserializeObject<Dictionary<string, Staff>>(response);
                }

                PopulateGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching staff from Firebase: " + ex.Message);
                staffData = new Dictionary<string, Staff>();
            }
        }

        private void PopulateGrid()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add("StaffID", "Staff ID");
            dgv.Columns["StaffID"].Visible = false;
            dgv.Columns.Add("Name", "Name");
            dgv.Columns.Add("FatherName", "Father Name");
            dgv.Columns.Add("Phone", "Phone Number");
            dgv.Columns.Add("Salary", "Salary");
            dgv.Columns.Add("Address", "Address");

            DataGridViewTextBoxColumn actionsCol = new DataGridViewTextBoxColumn
            {
                Name = "Actions",
                HeaderText = "Actions",
                Width = 120
            };
            dgv.Columns.Add(actionsCol);

            dgv.Rows.Clear();
            foreach (var kvp in staffData)
            {
                var s = kvp.Value;
                dgv.Rows.Add(kvp.Key, s.Name, s.FatherName, s.Phone, s.Salary, s.Address, "");
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string filter = searchBox.Text.Trim().ToLower();
            dgv.Rows.Clear();

            foreach (var kvp in staffData)
            {
                var s = kvp.Value;
                if (s.Name.ToLower().Contains(filter) || s.FatherName.ToLower().Contains(filter) || s.Phone.ToLower().Contains(filter))
                {
                    dgv.Rows.Add(kvp.Key, s.Name, s.FatherName, s.Phone, s.Salary, s.Address, "");
                }
            }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var icon1 = Properties.Resources.verification;
                var icon2 = Properties.Resources.edit;
                var icon3 = Properties.Resources.delete;

                int icon1X = e.CellBounds.Left + IconSpacing;
                int icon1Y = e.CellBounds.Top + (e.CellBounds.Height - IconSize) / 2;

                e.Graphics.DrawImage(icon1, new Rectangle(icon1X, icon1Y, IconSize, IconSize));
                e.Graphics.DrawImage(icon2, new Rectangle(icon1X + IconSize + IconSpacing, icon1Y, IconSize, IconSize));
                e.Graphics.DrawImage(icon3, new Rectangle(icon1X + 2 * (IconSize + IconSpacing), icon1Y, IconSize, IconSize));

                e.Handled = true;
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "Actions") return;

            var cellBounds = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            int clickX = dgv.PointToClient(Cursor.Position).X - cellBounds.Left;

            int icon1Start = IconSpacing;
            int icon1End = icon1Start + IconSize;
            int icon2Start = icon1End + IconSpacing;
            int icon2End = icon2Start + IconSize;
            int icon3Start = icon2End + IconSpacing;
            int icon3End = icon3Start + IconSize;

            string staffId = dgv.Rows[e.RowIndex].Cells["StaffID"].Value?.ToString();

            if (staffId == null || !staffData.ContainsKey(staffId)) return;

            if (clickX >= icon1Start && clickX <= icon1End)
            {
                // Verification
                var viewer = new StaffAttendanceViewer(staffId, staffData[staffId].Name);
                viewer.Show();
            }
            else if (clickX >= icon2Start && clickX <= icon2End)
            {
                // Edit
                var editForm = new StaffEditForm(staffId, staffData[staffId]);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Update Firebase
                    _ = UpdateStaffToFirebaseAsync(staffId, staffData[staffId]);
                    PopulateGrid();
                }
            }
            else if (clickX >= icon3Start && clickX <= icon3End)
            {
                // Delete
                DialogResult confirm = MessageBox.Show($"Delete staff '{staffId}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                {
                    staffData.Remove(staffId);
                    _ = DeleteStaffFromFirebaseAsync(staffId);
                    PopulateGrid();
                }
            }
        }

        private async Task UpdateStaffToFirebaseAsync(string staffId, Staff staff)
        {
            try
            {
                string url = $"YOUR_API_KEY_HERE{staffId}.json";
                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(staff);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                        MessageBox.Show("Failed to update staff in Firebase.", "Firebase Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating staff in Firebase: " + ex.Message);
            }
        }

        private async Task DeleteStaffFromFirebaseAsync(string staffId)
        {
            try
            {
                string url = $"YOUR_API_KEY_HERE{staffId}.json";
                using (var client = new HttpClient())
                {
                    var response = await client.DeleteAsync(url);
                    if (!response.IsSuccessStatusCode)
                        MessageBox.Show("Could not delete staff from Firebase.", "Firebase Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting staff from Firebase: " + ex.Message);
            }
        }
    }
}
