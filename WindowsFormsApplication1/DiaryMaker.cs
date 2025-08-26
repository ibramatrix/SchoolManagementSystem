using Siticone.Desktop.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Google.Cloud.Storage.V1;

namespace WindowsFormsApplication1
{


    public partial class DiaryMaker : Form
    {
        private SiticoneComboBox siticoneComboBoxClass;
        private SiticoneTextBox txtDiary;
        private SiticoneDateTimePicker datePicker;
        private SiticoneButton btnSelectImages, btnUpload;
        private FlowLayoutPanel flowImages;
        private Label lblStatus;

        private List<string> selectedImagePaths = new List<string>();

        // Replace with your Firebase URLs and Auth
        private static readonly string firebaseDbUrl = "YOUR_API_KEY_HERE";
        private static readonly string firebaseStorageBucket = "YOUR_API_KEY_HERE";
        private static readonly string firebaseAuth = "YOUR_API_KEY_HERE";

        private readonly string serviceAccountJsonPath = @"YOUR JSON FILE PATH";
        private StorageClient storageClient;

        public DiaryMaker()
        {
            InitializeComponent();
            SetupUI();
            LoadClasses();
        }

        private void InitializeStorageClient()
        {
            storageClient = StorageClient.Create(
                Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(serviceAccountJsonPath));

            // Debug: list buckets in your project
            var buckets = storageClient.ListBuckets("YOUR_API_KEY_HERE");
            foreach (var bucket in buckets)
            {
                Console.WriteLine("Found bucket: " + bucket.Name);
            }
        }


        private void SetupUI()
        {
            this.Text = "Diary Maker";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font controlFont = new Font("Segoe UI", 11);

            // GroupBox for Diary Entry Details
            var grpDiaryDetails = new GroupBox()
            {
                Text = "Diary Entry",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(660, 320),
                Location = new Point(15, 15),
            };
            this.Controls.Add(grpDiaryDetails);

            // Class combobox label
            var lblClass = new Label()
            {
                Text = "Select Class:",
                Location = new Point(20, 35),
                ForeColor = Color.White,
                Font = labelFont,
                AutoSize = true
            };
            grpDiaryDetails.Controls.Add(lblClass);

            // Class combobox
            siticoneComboBoxClass = new SiticoneComboBox
            {
                Location = new Point(120, 28),
                Size = new Size(200, 36),
                BorderRadius = 8,
                Font = controlFont,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            grpDiaryDetails.Controls.Add(siticoneComboBoxClass);

            // Date picker label
            var lblDate = new Label()
            {
                Text = "Select Date:",
                Location = new Point(350, 35),
                ForeColor = Color.White,
                Font = labelFont,
                AutoSize = true
            };
            grpDiaryDetails.Controls.Add(lblDate);

            // Date picker
            datePicker = new SiticoneDateTimePicker
            {
                Format = DateTimePickerFormat.Long,
                Size = new Size(250, 36),
                Location = new Point(440, 28),
                BorderRadius = 8,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = controlFont
            };
            grpDiaryDetails.Controls.Add(datePicker);

            // Diary Textbox label
            var lblDiaryText = new Label()
            {
                Text = "Diary Text:",
                Location = new Point(20, 80),
                ForeColor = Color.White,
                Font = labelFont,
                AutoSize = true
            };
            grpDiaryDetails.Controls.Add(lblDiaryText);

            // Diary Textbox
            txtDiary = new SiticoneTextBox
            {
                PlaceholderText = "Write your diary entry here...",
                Multiline = true,
                Size = new Size(610, 180),
                Location = new Point(20, 105),
                BorderRadius = 8,
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = controlFont,
                ScrollBars = ScrollBars.Vertical
            };
            grpDiaryDetails.Controls.Add(txtDiary);

            // GroupBox for Images
            var grpImages = new GroupBox()
            {
                Text = "Images",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(660, 210),
                Location = new Point(15, 350),
            };
            this.Controls.Add(grpImages);

            // Button to select images
            btnSelectImages = new SiticoneButton
            {
                Text = "📁 Select Images",
                Size = new Size(150, 40),
                Location = new Point(20, 30),
                BorderRadius = 8,
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSelectImages.Click += BtnSelectImages_Click;
            grpImages.Controls.Add(btnSelectImages);

            // FlowLayoutPanel to show image thumbnails
            flowImages = new FlowLayoutPanel
            {
                Location = new Point(20, 80),
                Size = new Size(610, 110),
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpImages.Controls.Add(flowImages);

            // Upload button
            btnUpload = new SiticoneButton
            {
                Text = "⬆️ Upload Diary",
                Size = new Size(200, 50),
                Location = new Point(475, 570),
                BorderRadius = 10,
                FillColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnUpload.Click += BtnUpload_Click;
            this.Controls.Add(btnUpload);

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                ForeColor = Color.LightGreen,
                Location = new Point(15, 580),
                Size = new Size(450, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblStatus);
        }


        private void LoadClasses()
        {
            // Example hardcoded classes, replace with your actual source (like allClasses.Keys)
            siticoneComboBoxClass.Items.Clear();
            siticoneComboBoxClass.Items.Add("P_G");
            siticoneComboBoxClass.Items.Add("Class 1");
            siticoneComboBoxClass.Items.Add("Class 2");
            siticoneComboBoxClass.Items.Add("Class 3");
            siticoneComboBoxClass.SelectedIndex = 0; // default selected
        }

        private void BtnSelectImages_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePaths.Clear();
                    flowImages.Controls.Clear();

                    foreach (var path in dlg.FileNames)
                    {
                        selectedImagePaths.Add(path);

                        PictureBox pic = new PictureBox
                        {
                            Image = Image.FromFile(path),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Width = 100,
                            Height = 100,
                            Margin = new Padding(5)
                        };
                        flowImages.Controls.Add(pic);
                    }
                }
            }
        }

        private async void BtnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDiary.Text))
            {
                MessageBox.Show("Please enter diary text!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (siticoneComboBoxClass.SelectedItem == null)
            {
                MessageBox.Show("Please select a class!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnUpload.Enabled = false;
            lblStatus.ForeColor = Color.LightGreen;
            lblStatus.Text = "Uploading diary... Please wait.";

            try
            {
                var uploadedImageUrls = new List<string>();
                foreach (var imgPath in selectedImagePaths)
                {
                    string url = await UploadImageToFirebase(imgPath);
                    uploadedImageUrls.Add(url);
                }

                var diaryEntry = new DiaryEntry
                {
                    ClassName = siticoneComboBoxClass.SelectedItem.ToString(),
                    Text = txtDiary.Text.Trim(),
                    Date = datePicker.Value.ToString("yyyy-MM-dd"),
                    Images = uploadedImageUrls
                };

                await UploadDiaryEntry(diaryEntry);

                lblStatus.ForeColor = Color.LightGreen;
                lblStatus.Text = "Diary uploaded successfully!";

                // Reset UI
                txtDiary.Clear();
                selectedImagePaths.Clear();
                flowImages.Controls.Clear();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = $"Upload failed: {ex.Message}";
                Console.WriteLine(lblStatus.Text.ToString());
            }
            finally
            {
                btnUpload.Enabled = true;
            }
        }

        private async Task<string> UploadImageToFirebase(string imagePath)
        {
            if (storageClient == null)
                InitializeStorageClient();

            string bucketName = firebaseStorageBucket;
            string objectName = $"diary_images/{Guid.NewGuid()}{Path.GetExtension(imagePath)}";

            // Generate a new token (UUID) for download URL
            string downloadToken = Guid.NewGuid().ToString();

            var obj = new Google.Apis.Storage.v1.Data.Object()
            {
                Bucket = bucketName,
                Name = objectName,
                Metadata = new Dictionary<string, string>()
        {
            { "firebaseStorageDownloadTokens", downloadToken }
        }
            };

            using (var fileStream = File.OpenRead(imagePath))
            {
                var uploadedObj = await storageClient.UploadObjectAsync(obj, fileStream);
                // uploadedObj now contains the uploaded object metadata
            }

            string downloadUrl = $"YOUR_API_KEY_HERE{bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media&token={downloadToken}";

            return downloadUrl;
        }





        private async Task UploadDiaryEntry(DiaryEntry diary)
        {
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(diary);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{firebaseDbUrl}.json{firebaseAuth}", content);
                response.EnsureSuccessStatusCode();
            }
        }
    }

    public class DiaryEntry
    {
        public string ClassName { get; set; }  // added class name property
        public string Date { get; set; }
        public string Text { get; set; }
        public List<string> Images { get; set; }
    }
}
