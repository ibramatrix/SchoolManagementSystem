using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;

namespace WindowsFormsApplication1
{
    public partial class DairyViewer : Form
    {
        private SiticoneTabControl tabControl;
        private Dictionary<string, Dictionary<string, DiaryEntry>> diariesByClass = new Dictionary<string, Dictionary<string, DiaryEntry>>();
        private static readonly string firebaseDbUrl = "YOUR_API_KEY_HERE";

        public DairyViewer()
        {
            InitializeComponent();
            InitializeUI();
            LoadDiariesAsync();
        }

        private void InitializeUI()
        {
            this.Text = "Diary Viewer";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            tabControl = new SiticoneTabControl
            {
                Size = new Size(860, 620),
                Location = new Point(15, 15),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White,
                ItemSize = new Size(150, 40)
                
            };
            tabControl.TabButtonHoverState = new Siticone.Desktop.UI.WinForms.SiticoneTabControl.TabButtonState()
            {
                FillColor = Color.FromArgb(50, 50, 50),
                BorderColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),

            };

            tabControl.TabButtonSelectedState = new Siticone.Desktop.UI.WinForms.SiticoneTabControl.TabButtonState()
            {
                FillColor = Color.FromArgb(0, 120, 215),
                BorderColor = Color.FromArgb(0, 84, 150),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),

            };
            this.Controls.Add(tabControl);
        }

        private async void LoadDiariesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = await client.GetStringAsync(firebaseDbUrl);
                    // Deserialize as Dictionary<id, DiaryEntry>
                    var allDiaries = JsonConvert.DeserializeObject<Dictionary<string, DiaryEntry>>(json);

                    if (allDiaries == null || allDiaries.Count == 0)
                    {
                        MessageBox.Show("No diaries found!");
                        return;
                    }

                    // Group by class
                    diariesByClass.Clear();
                    foreach (var kvp in allDiaries)
                    {
                        var id = kvp.Key;
                        var diary = kvp.Value;
                        if (!diariesByClass.ContainsKey(diary.ClassName))
                            diariesByClass[diary.ClassName] = new Dictionary<string, DiaryEntry>();

                        diariesByClass[diary.ClassName][id] = diary;
                    }

                    PopulateTabs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load diaries: " + ex.Message);
            }
        }

        private void PopulateTabs()
        {
            tabControl.TabPages.Clear();

            foreach (var classKvp in diariesByClass)
            {
                var className = classKvp.Key;
                var diaryDict = classKvp.Value;

                var tabPage = new TabPage(className)
                {
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                };

                // Scrollable panel for diaries
                var scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.FromArgb(45, 45, 48),
                };
                tabPage.Controls.Add(scrollPanel);

                int y = 10;
                foreach (var diaryKvp in diaryDict)
                {
                    string diaryId = diaryKvp.Key;
                    DiaryEntry diary = diaryKvp.Value;

                    var diaryCard = CreateDiaryCard(diaryId, diary);
                    diaryCard.Location = new Point(10, y);
                    scrollPanel.Controls.Add(diaryCard);
                    y += diaryCard.Height + 10;
                }

                tabControl.TabPages.Add(tabPage);
            }
        }

        private Panel CreateDiaryCard(string diaryId, DiaryEntry diary)
        {
            var card = new Panel
            {
                Size = new Size(810, 140),
                BackColor = Color.FromArgb(60, 60, 63),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblDate = new Label
            {
                Text = diary.Date,
                Location = new Point(10, 10),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true
            };
            card.Controls.Add(lblDate);

            var txtDiary = new TextBox
            {
                Text = diary.Text,
                Location = new Point(10, 30),
                Size = new Size(600, 80),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(40, 40, 43),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)  // <-- Add this line

            };
            card.Controls.Add(txtDiary);

            var btnShowImages = new SiticoneButton
            {
                Text = "Show Images",
                Location = new Point(620, 30),
                Size = new Size(170, 30),
                BorderRadius = 8,
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnShowImages.Click += (s, e) => ShowImages(diary.Images);
            card.Controls.Add(btnShowImages);

            var btnEdit = new SiticoneButton
            {
                Text = "Edit",
                Location = new Point(620, 70),
                Size = new Size(80, 30),
                BorderRadius = 8,
                FillColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            };
            btnEdit.Click += async (s, e) => await EditDiaryAsync(diaryId, diary);
            card.Controls.Add(btnEdit);

            var btnDelete = new SiticoneButton
            {
                Text = "Delete",
                Location = new Point(710, 70),
                Size = new Size(80, 30),
                BorderRadius = 8,
                FillColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White
            };
            btnDelete.Click += async (s, e) => await DeleteDiaryAsync(diaryId);
            card.Controls.Add(btnDelete);

            return card;
        }

        private void ShowImages(List<string> images)
        {
            if (images == null || images.Count == 0)
            {
                MessageBox.Show("No images for this diary.");
                return;
            }

            // Simple image viewer dialog
            Form imageViewer = new Form
            {
                Text = "Diary Images",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
            };

            foreach (var imgUrl in images)
            {
                var pb = new PictureBox
                {
                    Size = new Size(300, 300),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(10)
                };

                pb.LoadCompleted += (sender, e) =>
                {
                    if (e.Error != null || e.Cancelled)
                    {
                        pb.Visible = false;
                        Console.WriteLine($"Failed to load image: {imgUrl} Error: {e.Error?.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"Image loaded successfully: {imgUrl}");
                    }
                };

                try
                {
                    pb.LoadAsync(imgUrl);
                }
                catch
                {
                    pb.Visible = false;
                }

                flow.Controls.Add(pb);
            }

            imageViewer.Controls.Add(flow);
            imageViewer.ShowDialog();
        }


        private async Task EditDiaryAsync(string diaryId, DiaryEntry diary)
        {
            // Show simple input dialog to edit diary text only
            using (var editForm = new Form()
            {
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                Text = "Edit Diary Entry",
                BackColor = Color.FromArgb(30, 30, 30)
            })
            {
                var txtEdit = new TextBox
                {
                    Multiline = true,
                    Size = new Size(560, 250),
                    Location = new Point(10, 10),
                    Text = diary.Text,
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular)  // <-- Add this line

                };
                editForm.Controls.Add(txtEdit);

                var btnSave = new SiticoneButton
                {
                    Text = "Save",
                    Location = new Point(450, 300),
                    Size = new Size(120, 40),
                    FillColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    BorderRadius = 10
                };
                editForm.Controls.Add(btnSave);

                btnSave.Click += async (s, e) =>
                {
                    // Update diary text and send update to Firebase
                    diary.Text = txtEdit.Text.Trim();

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var json = JsonConvert.SerializeObject(diary);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var response = await client.PutAsync($"YOUR_API_KEY_HERE{diaryId}.json", content);
                            response.EnsureSuccessStatusCode();

                            MessageBox.Show("Diary updated!");
                            editForm.Close();

                            // Reload diaries after edit
                            LoadDiariesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Update failed: " + ex.Message);
                    }
                };

                editForm.ShowDialog();
            }
        }

        private async Task DeleteDiaryAsync(string diaryId)
        {
            var confirm = MessageBox.Show("Are you sure you want to delete this diary entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.DeleteAsync($"YOUR_API_KEY_HERE{diaryId}.json");
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Diary deleted!");
                    LoadDiariesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete failed: " + ex.Message);
            }
        }
    }

   
}
