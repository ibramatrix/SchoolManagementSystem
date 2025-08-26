using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class NoteMaker : Form
    {
        private SiticoneTextBox txtNoteTitle;
        private SiticoneComboBox cmbClass;
        private SiticoneButton btnSelectFile, btnUploadNote, btnManageNotes;
        private Label lblFileName;
        private string selectedFilePath;

        public NoteMaker()
        {
            InitializeComponent();
            SetupUI();
            LoadClasses();
        }

        private void SetupUI()
        {
            this.Text = "Note Maker";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(30, 30, 30);

            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font controlFont = new Font("Segoe UI", 11);

            // Note Title
            Label lblTitle = new Label()
            {
                Text = "Note Title:",
                ForeColor = Color.White,
                Location = new Point(30, 30),
                AutoSize = true,
                Font = labelFont
            };
            this.Controls.Add(lblTitle);

            txtNoteTitle = new SiticoneTextBox()
            {
                Location = new Point(120, 25),
                Size = new Size(400, 36),
                BorderRadius = 8,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = controlFont,
                PlaceholderText = "Enter note title"
            };
            this.Controls.Add(txtNoteTitle);

            // Class ComboBox
            Label lblClass = new Label()
            {
                Text = "Class:",
                ForeColor = Color.White,
                Location = new Point(30, 80),
                AutoSize = true,
                Font = labelFont
            };
            this.Controls.Add(lblClass);

            cmbClass = new SiticoneComboBox()
            {
                Location = new Point(120, 75),
                Size = new Size(200, 36),
                BorderRadius = 8,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = controlFont,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbClass);

            // Select File Button
            btnSelectFile = new SiticoneButton()
            {
                Text = "📁 Select File",
                Size = new Size(150, 40),
                Location = new Point(120, 130),
                BorderRadius = 8,
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSelectFile.Click += BtnSelectFile_Click;
            this.Controls.Add(btnSelectFile);

            lblFileName = new Label()
            {
                Text = "No file selected",
                ForeColor = Color.LightGray,
                Location = new Point(280, 138),
                AutoSize = true,
                Font = labelFont
            };
            this.Controls.Add(lblFileName);

            // Upload Note Button
            btnUploadNote = new SiticoneButton()
            {
                Text = "⬆️ Upload Note",
                Size = new Size(150, 45),
                Location = new Point(120, 200),
                BorderRadius = 10,
                FillColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnUploadNote.Click += BtnUploadNote_Click;
            this.Controls.Add(btnUploadNote);

            // Manage Notes Button
            btnManageNotes = new SiticoneButton()
            {
                Text = "📝 Manage Notes",
                Size = new Size(150, 45),
                Location = new Point(300, 200),
                BorderRadius = 10,
                FillColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnManageNotes.Click += BtnManageNotes_Click;
            this.Controls.Add(btnManageNotes);
        }

        private void LoadClasses()
        {
            // Example hardcoded classes
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

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "All Files|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = dlg.FileName;
                    lblFileName.Text = Path.GetFileName(selectedFilePath);
                }
            }
        }

        private async void BtnUploadNote_Click(object sender, EventArgs e)
        {
            string noteTitle = txtNoteTitle.Text.Trim();
            string className = cmbClass.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(noteTitle))
            {
                MessageBox.Show("Please enter note title!");
                return;
            }
            if (string.IsNullOrWhiteSpace(selectedFilePath))
            {
                MessageBox.Show("Please select a file!");
                return;
            }

            btnUploadNote.Enabled = false;
            btnUploadNote.Text = "Uploading...";

            try
            {
                await FirebaseHelper.SaveNoteAsync(className, noteTitle, selectedFilePath);
                MessageBox.Show("Note uploaded successfully!");
                txtNoteTitle.Clear();
                lblFileName.Text = "No file selected";
                selectedFilePath = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Upload failed: " + ex.Message);
            }
            finally
            {
                btnUploadNote.Enabled = true;
                btnUploadNote.Text = "⬆️ Upload Note";
            }
        }

        private void BtnManageNotes_Click(object sender, EventArgs e)
        {
            NoteManager manager = new NoteManager();
            manager.Show();
        }
    }
}
