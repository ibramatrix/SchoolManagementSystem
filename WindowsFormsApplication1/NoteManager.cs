using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class NoteManager : Form
    {
        private SiticoneComboBox cmbClass;
        private SiticoneDataGridView dgvNotes;
        private SiticoneButton btnRefresh, btnDelete, btnUpdateFile;
        private string selectedFilePath;

        public NoteManager()
        {
            InitializeComponent();
            SetupUI();
            LoadClasses();
        }

        private void SetupUI()
        {
            this.Text = "Note Manager";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);

            Label lblClass = new Label()
            {
                Text = "Select Class:",
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true,
                Font = labelFont
            };
            this.Controls.Add(lblClass);

            cmbClass = new SiticoneComboBox()
            {
                Location = new Point(120, 15),
                Size = new Size(200, 36),
                BorderRadius = 8,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbClass.SelectedIndexChanged += CmbClass_SelectedIndexChanged;
            this.Controls.Add(cmbClass);

            dgvNotes = new SiticoneDataGridView()
            {
                Location = new Point(20, 70),
                Size = new Size(640, 320),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgvNotes.Columns.Add("Title", "Title");
            dgvNotes.Columns.Add("FileUrl", "File URL");
            this.Controls.Add(dgvNotes);

            btnRefresh = new SiticoneButton()
            {
                Text = "🔄 Refresh",
                Size = new Size(120, 40),
                Location = new Point(20, 400),
                FillColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            btnDelete = new SiticoneButton()
            {
                Text = "🗑 Delete",
                Size = new Size(120, 40),
                Location = new Point(160, 400),
                FillColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);

            btnUpdateFile = new SiticoneButton()
            {
                Text = "📁 Update File",
                Size = new Size(150, 40),
                Location = new Point(300, 400),
                FillColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            btnUpdateFile.Click += BtnUpdateFile_Click;
            this.Controls.Add(btnUpdateFile);
        }

        private void LoadClasses()
        {
            cmbClass.Items.Clear();
            cmbClass.Items.Clear();
            cmbClass.Items.Add("P_G");
            cmbClass.Items.Add("K_G");
            cmbClass.Items.Add("NURSERY");
            cmbClass.Items.Add("ONE");
            cmbClass.Items.Add("TWO");
            cmbClass.Items.Add("THREE");
            cmbClass.Items.Add("FOUR");
            cmbClass.Items.Add("FIVE");
            cmbClass.Items.Add("SIX");
            cmbClass.Items.Add("SEVEN");
            cmbClass.Items.Add("EIGHT");
            cmbClass.Items.Add("NINETH");
            cmbClass.Items.Add("TENTH");
            cmbClass.SelectedIndex = 0;
        }

        private async void CmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadNotes();
        }

        private async Task LoadNotes()
        {
            if (cmbClass.SelectedItem == null) return;

            dgvNotes.Rows.Clear();
            string className = cmbClass.SelectedItem.ToString();
            var notes = await FirebaseHelper.GetNotesAsync(className);

            foreach (var n in notes)
            {
                dgvNotes.Rows.Add(n.Key, n.Value.FileUrl);
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadNotes();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvNotes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a note first!");
                return;
            }

            string title = dgvNotes.SelectedRows[0].Cells["Title"].Value.ToString();
            string className = cmbClass.SelectedItem.ToString();

            await FirebaseHelper.DeleteNoteAsync(className, title);
            MessageBox.Show("Note deleted!");
            await LoadNotes();
        }

        private void BtnUpdateFile_Click(object sender, EventArgs e)
        {
            if (dgvNotes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a note first!");
                return;
            }

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "All Files|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = dlg.FileName;
                    UpdateSelectedNoteFile();
                }
            }
        }

        private async void UpdateSelectedNoteFile()
        {
            string title = dgvNotes.SelectedRows[0].Cells["Title"].Value.ToString();
            string className = cmbClass.SelectedItem.ToString();

            try
            {
                await FirebaseHelper.UpdateNoteAsync(className, title, selectedFilePath);
                MessageBox.Show("File updated successfully!");
                await LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed: " + ex.Message);
            }
        }
    }
}
