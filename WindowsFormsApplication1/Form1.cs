using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{


    public partial class Form1 : Form
    {
        private TimetableManagerForm timetableControl;

        private FingerprintEnroller fingerprintEnrollerControl;

        private FirebaseClient firebaseClient = new FirebaseClient("YOUR_API_KEY_HERE");
        public Form1()
        {
            InitializeComponent();
            CheckFirstTimeSync();




        }

        private async void Form1_Load(object sender, EventArgs e)
        {
        }

        public async Task LoadEventCards(string year, string month)
        {
            var events = await firebaseClient
        .Child("EventList")
        .Child("StaffEvents")    
        .Child(year)
        .Child(month)
        .OnceAsync<SchoolEvent>();

            eventFlowPanel.Controls.Clear();

            foreach (var ev in events)
            {
                string firebaseKey = ev.Key;
                string title = ev.Object.Title;
                string time = ev.Object.Time;
                string date = ev.Object.Date;
                string status = ev.Object.Status;

                AddEventCard(title, time, date, status, firebaseKey, year, month);
            }
        }


        public void AddEventCard(string title, string time, string date, string status, string firebaseKey, string year, string month)
        {
            // EventCard (SiticoneGradientPanel)
            var eventCard = new Siticone.Desktop.UI.WinForms.SiticoneGradientPanel
            {
                Size = new Size(328, 79),
                BorderRadius = 10,
                FillColor = Color.FromArgb(253, 121, 168),
                FillColor2 = Color.FromArgb(232, 67, 147),
                BackColor = Color.Transparent,
                Margin = new Padding(10, 10, 10, 10),
                Cursor = Cursors.Hand
            };

            eventCard.Tag = new EventMeta
            {
                Key = firebaseKey,
                Year = year,
                Month = month
            };

            // Title Label
            var titleLabel = new Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(9, 4),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Time Panel
            var timePanel = new Siticone.Desktop.UI.WinForms.SiticonePanel
            {
                Size = new Size(98, 41),
                Location = new Point(9, 33),
                BackColor = Color.Transparent
            };

            var timeIcon = new Siticone.Desktop.UI.WinForms.SiticonePictureBox
            {
                Location = new Point(3, 12),
                Size = new Size(25, 25),
                ImageRotate = 0F,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.clock ?? null // add null check fallback if needed
            };

            var timeLabel = new Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel
            {
                Text = time,
                Font = new Font("Segoe UI", 9.75F),
                ForeColor = Color.White,
                Location = new Point(33, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            timePanel.Controls.Add(timeIcon);
            timePanel.Controls.Add(timeLabel);

            // Date Panel
            var datePanel = new Siticone.Desktop.UI.WinForms.SiticonePanel
            {
                Size = new Size(81, 44),
                Location = new Point(112, 32),
                BackColor = Color.Transparent
            };

            var dateIcon = new Siticone.Desktop.UI.WinForms.SiticonePictureBox
            {
                Location = new Point(3, 8),
                Size = new Size(28, 31),
                ImageRotate = 0F,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.schedule__1_ ?? null
            };

            var dateLabel = new Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel
            {
                Text = date,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(33, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            datePanel.Controls.Add(dateIcon);
            datePanel.Controls.Add(dateLabel);

            // Status Panel
            var statusPanel = new Siticone.Desktop.UI.WinForms.SiticonePanel
            {
                Size = new Size(79, 71),
                Location = new Point(235, 4),
                BackColor = Color.Transparent
            };

            var statusIcon = new Siticone.Desktop.UI.WinForms.SiticonePictureBox
            {
                Location = new Point(18, 4),
                Size = new Size(40, 40),
                ImageRotate = 0F,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.clipboard ?? null
            };

            var statusLabel = new Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel
            {
                Text = status,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                ForeColor = Color.FromArgb(85, 239, 196),
                Location = new Point(5, 46),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            statusPanel.Controls.Add(statusIcon);
            statusPanel.Controls.Add(statusLabel);

            // Combine all controls into eventCard
            eventCard.Controls.Add(titleLabel);
            eventCard.Controls.Add(timePanel);
            eventCard.Controls.Add(datePanel);
            eventCard.Controls.Add(statusPanel);


            eventCard.Click += EventCard_Click;

            // Add eventCard to FlowLayoutPanel
            AttachClickHandlerRecursively(eventCard, EventCard_Click);

            eventFlowPanel.Controls.Add(eventCard);
        }


        private void AttachClickHandlerRecursively(Control control, EventHandler handler)
        {
            control.Click += handler;

            foreach (Control child in control.Controls)
            {
                AttachClickHandlerRecursively(child, handler);
            }
        }

        private void EventCard_Click(object sender, EventArgs e)
        {
            Control clickedControl = sender as Control;

            // Walk up the control tree to find the SiticoneGradientPanel
            while (clickedControl != null && !(clickedControl is SiticoneGradientPanel))
            {
                clickedControl = clickedControl.Parent;
            }

            SiticoneGradientPanel card = clickedControl as SiticoneGradientPanel;
            if (card == null)
            {
                MessageBox.Show("Could not identify the event card.");
                return;
            }

            EventMeta meta = card.Tag as EventMeta;
            if (meta == null)
            {
                MessageBox.Show("Could not identify event metadata.");
                return;
            }

            // Extract title
            string title = card.Controls.OfType<SiticoneHtmlLabel>()
                .FirstOrDefault(l => l.Location == new Point(9, 4))?.Text ?? "";

            // Extract time
            string time = card.Controls.OfType<SiticonePanel>()
                .SelectMany(p => p.Controls.OfType<SiticoneHtmlLabel>())
                .FirstOrDefault(l => l.Text.Contains(":"))?.Text ?? "";

            // Extract date
            string date = card.Controls.OfType<SiticonePanel>()
                .SelectMany(p => p.Controls.OfType<SiticoneHtmlLabel>())
                .FirstOrDefault(l => l.Text.Contains("Jan") || l.Text.Contains("Feb") ||
                                     l.Text.Contains("Mar") || l.Text.Contains("Apr") ||
                                     l.Text.Contains("May") || l.Text.Contains("Jun") ||
                                     l.Text.Contains("Jul") || l.Text.Contains("Aug") ||
                                     l.Text.Contains("Sep") || l.Text.Contains("Oct") ||
                                     l.Text.Contains("Nov") || l.Text.Contains("Dec"))?.Text ?? "";

            // Extract status
            string status = card.Controls.OfType<SiticonePanel>()
                .SelectMany(p => p.Controls.OfType<SiticoneHtmlLabel>())
                .FirstOrDefault(l => l.Location == new Point(5, 46))?.Text ?? "Upcoming";

            // Open update form
            var updateForm = new UpdateEventForm(meta.Key, meta.Year, meta.Month, title, date, time, status);
            updateForm.FormClosed += async (s, args) => await LoadEventCards(meta.Year, meta.Month);
            updateForm.ShowDialog();
        }




        private string GetLabelFrom(Control parent, Point labelLocation, int panelOffsetX = 0)
        {
            foreach (Control control in parent.Controls)
            {
                if (panelOffsetX == 0 || Math.Abs(control.Location.X - panelOffsetX) < 30)
                {
                    var label = control.Controls.OfType<SiticoneHtmlLabel>().FirstOrDefault(l => l.Location == labelLocation);
                    if (label != null) return label.Text;
                }
            }
            return string.Empty;
        }

        private async void CheckFirstTimeSync()
        {
            // Use Properties.Settings to store a one-time flag
          //  if (!Properties.Settings.Default.HasSyncedData)
          //  {
              //  await SyncStudentsToFirebase();
                //await SyncStaffToFirebase();

                // Set the flag so it won't run again
               // Properties.Settings.Default.HasSyncedData = true;
               // Properties.Settings.Default.Save();
            //}
        }

        /*
        private async Task SyncStudentsToFirebase()
        {
            string filePath = "Students.json";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Students.json not found!");
                return;
            }

            var json = File.ReadAllText(filePath);

            var allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);

            if (allClasses == null)
            {
                MessageBox.Show("No data to sync.");
                return;
            }

            foreach (var classEntry in allClasses)
            {
                string className = classEntry.Key;

                foreach (var studentEntry in classEntry.Value)
                {
                    string studentId = studentEntry.Key;
                    Student student = studentEntry.Value;

                    await firebaseClient
                        .Child("students")
                        .Child(className)
                        .Child(studentId)
                        .PutAsync(student);
                }
            }

            MessageBox.Show("✅ Students.json successfully synced to Firebase.");
        }

        private async Task SyncStaffToFirebase()
        {
            string filePath = "Staff.json";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Staff.json not found!");
                return;
            }

            var json = File.ReadAllText(filePath);

            var allStaff = JsonConvert.DeserializeObject<Dictionary<string, Staff>>(json);

            if (allStaff == null)
            {
                MessageBox.Show("No staff data to sync.");
                return;
            }

            foreach (var staffEntry in allStaff)
            {
                string staffId = staffEntry.Key;
                Staff staff = staffEntry.Value;

                await firebaseClient
                    .Child("Stafflist")
                    .Child(staffId)
                    .PutAsync(staff);
            }

            MessageBox.Show("✅ Staff.json successfully synced to Firebase.");
        }

        */
        private void removeBG(PictureBox pb,Label lbl)
        {
            var pos = this.PointToScreen(lbl.Location);
            pos = pb.PointToClient(pos);
            lbl.Parent = pb;
            lbl.Location = pos;
            lbl.BackColor = Color.Transparent;

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            sendWhatsapp("3221169411", "ASASHJAS");
        }

        private void sendWhatsapp(string number, string messages)
        {
            try
            {
                messages = "ASASHJAS";
                number = "3221169411";
                if (number == "")
                {
                    MessageBox.Show("NO NUM");



                }
                if( number.Length <= 0 )
                {
                    number = "+92" + number;
                }
                number = number.Replace(" ", "");
                System.Diagnostics.Process.Start("http://api.whatsapp.com/send?phone=" + number +"&text=" + messages);
            }
            catch (Exception){
                throw;
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            string className = LoggedInStaff.ManagesClass ?? "THREE";
            TestReportCreator reportForm = new TestReportCreator();
            reportForm.ShowDialog();
        }

        private void pbClients_Click(object sender, EventArgs e)
        {
          StudentMainPrin smp = new StudentMainPrin();
            smp.Show();
        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            NewStudentForm nsf = new NewStudentForm();
            nsf.Show();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pbOrder_Click(object sender, EventArgs e)
        {
            StaffListAdmin sla = new StaffListAdmin();
            sla.Show();
        }

        private void pbProfit_Click(object sender, EventArgs e)
        {
            ExpenseTracker et = new ExpenseTracker();
            et.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            FingerprintEnroller enrollForm = new FingerprintEnroller();
           // enrollForm.ShowDialog(); // use Show() if you don’t want it to block the main form
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            AttendanceViewer viewer = new AttendanceViewer();
            viewer.Show(); // Non-blocking, user can return to this form
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            FeeReminder feeReminder = new FeeReminder();
            feeReminder.Show();
        }

        private void pbSales_Click(object sender, EventArgs e)
        {
            RevenueForm revenueForm = new RevenueForm();
            revenueForm.Show();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            FingerprintManager fingerprintManager = new FingerprintManager();
            fingerprintManager.Show();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            NewStudentForm nsf = new NewStudentForm();
            nsf.Show();
        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            NewStaffForm nsf = new NewStaffForm();
            nsf.Show();
        }

       
            //await UploadStaffDataToFirebase();
            // await SyncStudentsToFirebase();
            private async void Form1_Load_1(object sender, EventArgs e)
        {


            CreateDashboardButtons();

            logopic.BackColor = Color.Transparent;
            logoutbtn.BackColor = Color.Transparent;
            sideex.BackColor    = Color.Transparent;   
            DashboardMenuBtn.BackColor = Color.Transparent;
            barcenter.BackColor = Color.Transparent;

            if(LoggedInStaff.Username != null)
            {
                TeacherName.Text = LoggedInStaff.Username.ToString();


            }

            string currentYear = DateTime.Now.Year.ToString();          // e.g., "2025"
            string currentMonth = DateTime.Now.ToString("MMMM");        // e.g., "August"

            await LoadEventCards(currentYear, currentMonth);



            //  TeacherName.Text = LoggedInStaff.Username.ToString();
            //  TeacherRole.Text = "You Manage: " + LoggedInStaff.ManagesClass.ToString();

            

            string role = LoggedInStaff.ManagesClass?.Trim().ToUpper() ?? "";

            

        }


        private async Task DownloadJsonNodeToFile(string firebaseNode, string localFileName)
        {
            try
            {
                var data = await firebaseClient
                    .Child(firebaseNode) // Ensure no trailing slash
                    .OnceSingleAsync<object>();

                if (data == null)
                {
                    MessageBox.Show($"⚠️ No data found in Firebase node '{firebaseNode}'");
                    return;
                }

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(localFileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error downloading {firebaseNode}: \n\n{ex.Message}");
            }
        }


        private async Task UploadStaffDataToFirebase()
        {
            string firebaseUrl = "YOUR_API_KEY_HERE";
            string localStaffPath = "Staff.json";

            if (!File.Exists(localStaffPath))
            {
                MessageBox.Show("⚠️ Staff.json file not found.");
                return;
            }

            try
            {
                // Read and parse the file first
                string staffJson = File.ReadAllText(localStaffPath);

                var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(staffJson);

                if (parsedJson == null)
                {
                    MessageBox.Show("⚠️ Staff.json is empty or malformed.");
                    return;
                }

                // Remove null values from all staff entries
                var cleanedJson = new Dictionary<string, Dictionary<string, object>>();

                foreach (var staff in parsedJson)
                {
                    var cleanValues = staff.Value
                        .Where(kv => kv.Value != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);

                    cleanedJson[staff.Key] = cleanValues;
                }

                string cleanedJsonString = JsonConvert.SerializeObject(cleanedJson, Formatting.Indented);

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(cleanedJsonString, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(firebaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Staff data uploaded safely to Firebase.");
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to upload staff data. Firebase response: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("🔥 Error during upload: " + ex.Message);
            }
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            TimetableManagerForm managerForm = new TimetableManagerForm();
            managerForm.Show();
        }

        private void button15_Click(object sender, EventArgs e)
        {
           
        }

        private void logoutbtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Goodbye");
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EventForm ec = new EventForm();
            ec.Show();
        }

        private void StudentsMenuBtn_Click(object sender, EventArgs e)
        {
            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }

            if (DashboardGroupBox.Visible == true)
            {
                
                DashboardGroupBox.Visible = false;
            }
            CreateStudentButtons();
            MainGroupBox.Controls.Clear(); // remove old content

            StudentMainPrin studentView = new StudentMainPrin();
            studentView.Dock = DockStyle.Fill;

            MainGroupBox.Controls.Add(studentView);
        }


        private void CreateStudentButtons()
        {
            SideExtraLayout.Controls.Clear();

            var buttonInfos = new[]
            {
          new { Text = "Add new Student", Image = Properties.Resources.students, FormType = typeof(NewStudentForm) },
        new { Text = "Create Report", Image = Properties.Resources.CreateReport, FormType = typeof(TestReportCreator) },
        new { Text = "Attendance", Image = Properties.Resources.ManageAttendance, FormType = typeof(AttendanceViewer) },
        new { Text = "Fees", Image = Properties.Resources.fees, FormType = typeof(FeeReminder) },
        new { Text = "View Reports", Image = Properties.Resources.fees, FormType = typeof(AllReports) }
    };

            foreach (var info in buttonInfos)
            {
                var btn = new SiticoneGradientButton()
                {
                    FillColor = Color.FromArgb(48, 51, 107),
                    FillColor2 = Color.FromArgb(72, 52, 212),
                    GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal,
                    BorderRadius = 9,
                    Size = new Size(168, 46),
                    ForeColor = Color.White,
                    Font = new Font("Pivot Classic", 9, FontStyle.Bold | FontStyle.Italic),
                    Text = info.Text,
                    ImageAlign = HorizontalAlignment.Left,
                    TextAlign = HorizontalAlignment.Left,
                    ImageSize = new Size(24, 24),
                    Margin = new Padding(5),
                    Image = info.Image
                };

                btn.Click += (s, e) =>
                {
                    Form form = (Form)Activator.CreateInstance(info.FormType);
                    form.Show();
                };

                SideExtraLayout.Controls.Add(btn);
            }
        }

        private void CreateTeacherButtons()
        {
            SideExtraLayout.Controls.Clear();

            var buttonInfos = new[]
            {
        new { Text = "Add New Teacher", Image = Properties.Resources.teacher, FormType = typeof(NewStaffForm) },   // Make sure you have these images in resources
        new { Text = "Attendance", Image = Properties.Resources.ManageAttendance, FormType = typeof(AllStaffAttendanceViewer) },
        new { Text = "Salary Management", Image = Properties.Resources.fees, FormType = typeof(StaffSalaryManager) }
    };

            foreach (var info in buttonInfos)
            {
                var btn = new SiticoneGradientButton()
                {
                    FillColor = Color.FromArgb(48, 51, 107),
                    FillColor2 = Color.FromArgb(72, 52, 212),
                    GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal,
                    BorderRadius = 9,
                    Size = new Size(168, 46),
                    ForeColor = Color.White,
                    Font = new Font("Pivot Classic", 9, FontStyle.Bold | FontStyle.Italic),
                    Text = info.Text,
                    ImageAlign = HorizontalAlignment.Left,
                    TextAlign = HorizontalAlignment.Left,
                    ImageSize = new Size(24, 24),
                    Margin = new Padding(5),
                    Image = info.Image
                };

                btn.Click += (s, e) =>
                {
                    Form form = (Form)Activator.CreateInstance(info.FormType);
                    form.Show();
                };

                SideExtraLayout.Controls.Add(btn);
            }
        }

        private void CreateFingerprintButtons()
        {
            SideExtraLayout.Controls.Clear();

            var buttonInfos = new[]
            {
          new { Text = "Fingerprint Manager", Image = Properties.Resources.touch_id, FormType = typeof(FingerprintManager)  }
    };

            foreach (var info in buttonInfos)
            {
                var btn = new SiticoneGradientButton()
                {
                    FillColor = Color.FromArgb(48, 51, 107),
                    FillColor2 = Color.FromArgb(72, 52, 212),
                    GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal,
                    BorderRadius = 9,
                    Size = new Size(168, 46),
                    ForeColor = Color.White,
                    Font = new Font("Pivot Classic", 9, FontStyle.Bold | FontStyle.Italic),
                    Text = info.Text,
                    ImageAlign = HorizontalAlignment.Left,
                    TextAlign = HorizontalAlignment.Left,
                    ImageSize = new Size(24, 24),
                    Margin = new Padding(5),
                    Image = info.Image
                };

                btn.Click += (s, e) =>
                {
                    Form form = (Form)Activator.CreateInstance(info.FormType);
                    form.Show();
                };

                SideExtraLayout.Controls.Add(btn);
            }
        }


        private void CreateDashboardButtons()
        {
            SideExtraLayout.Controls.Clear();

            var buttonInfos = new[]
            {
          new { Text = "Add New Event", Image = Properties.Resources.pin, FormType = typeof(EventForm) },
          new { Text = "Add Holiday", Image = Properties.Resources.fees, FormType = typeof(HolidayMarker) },
          new { Text = "Add Diary", Image = Properties.Resources.diary, FormType = typeof(DiaryMaker) },
          new { Text = "Manage Diary", Image = Properties.Resources.project, FormType = typeof(DairyViewer) },
                    new { Text = "Add Notes", Image = Properties.Resources.notepad, FormType = typeof(NoteMaker) }


    };

            foreach (var info in buttonInfos)
            {
                var btn = new SiticoneGradientButton()
                {
                    FillColor = Color.FromArgb(48, 51, 107),
                    FillColor2 = Color.FromArgb(72, 52, 212),
                    GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal,
                    BorderRadius = 9,
                    Size = new Size(168, 46),
                    ForeColor = Color.White,
                    Font = new Font("Pivot Classic", 9, FontStyle.Bold | FontStyle.Italic),
                    Text = info.Text,
                    ImageAlign = HorizontalAlignment.Left,
                    TextAlign = HorizontalAlignment.Left,
                    ImageSize = new Size(24, 24),
                    Margin = new Padding(5),
                    Image = info.Image
                };

                btn.Click += (s, e) =>
                {
                    Form form = (Form)Activator.CreateInstance(info.FormType);
                    form.Show();
                };

                SideExtraLayout.Controls.Add(btn);
            }
        }
        private void TeachersMenuBtn_Click(object sender, EventArgs e)
        {
            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }


            if (DashboardGroupBox.Visible == true)
            {
                DashboardGroupBox.Visible = false;
            }
            CreateTeacherButtons();
            MainGroupBox.Controls.Clear();
            var staffList = new StaffListAdmin();
            staffList.Dock = DockStyle.Fill;
            MainGroupBox.Controls.Add(staffList);
        }

        private void FingerprintMenuBtn_Click(object sender, EventArgs e)
        {
            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }

            if (DashboardGroupBox.Visible == true)
            {
                DashboardGroupBox.Visible = false;
            }
            MainGroupBox.Controls.Clear();

            if (fingerprintEnrollerControl != null)
            {
                
                fingerprintEnrollerControl.Dispose();
                fingerprintEnrollerControl = null;
            }

            fingerprintEnrollerControl = new FingerprintEnroller()
            {
                Dock = DockStyle.Fill
            };

            MainGroupBox.Controls.Add(fingerprintEnrollerControl);
            CreateFingerprintButtons();
        }

        private void CreateReportsButtons()
        {
            SideExtraLayout.Controls.Clear();

            var buttonInfos = new[]
            {
          new { Text = "Expenses", Image = Properties.Resources.students, FormType = typeof(ExpenseTracker) }
    };

            foreach (var info in buttonInfos)
            {
                var btn = new SiticoneGradientButton()
                {
                    FillColor = Color.FromArgb(48, 51, 107),
                    FillColor2 = Color.FromArgb(72, 52, 212),
                    GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal,
                    BorderRadius = 9,
                    Size = new Size(168, 46),
                    ForeColor = Color.White,
                    Font = new Font("Pivot Classic", 9, FontStyle.Bold | FontStyle.Italic),
                    Text = info.Text,
                    ImageAlign = HorizontalAlignment.Left,
                    TextAlign = HorizontalAlignment.Left,
                    ImageSize = new Size(24, 24),
                    Margin = new Padding(5),
                    Image = info.Image
                };

                btn.Click += (s, e) =>
                {
                    Form form = (Form)Activator.CreateInstance(info.FormType);
                    form.Show();
                };

                SideExtraLayout.Controls.Add(btn);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (fingerprintEnrollerControl != null)
                fingerprintEnrollerControl = null;
        }

        private void TimetableMenuBtn_Click(object sender, EventArgs e)
        {
            SideExtraLayout.Controls.Clear();

            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }

            if (DashboardGroupBox.Visible == true)
            {
                DashboardGroupBox.Visible = false;
            }
            if (timetableControl == null)
                timetableControl = new TimetableManagerForm();

            MainGroupBox.Controls.Clear();
            MainGroupBox.Controls.Add(timetableControl);
            timetableControl.Dock = DockStyle.Fill;
            timetableControl.BringToFront();
        }

        private void ReportsMenuBtn_Click(object sender, EventArgs e)
        {
            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }

            if (DashboardGroupBox.Visible == true)
            {
                DashboardGroupBox.Visible = false;
            }

            var revUC = new RevenueForm();
            MainGroupBox.Controls.Clear();
            MainGroupBox.Controls.Add(revUC);

            CreateReportsButtons();
        }

        private void pictureBox4_Click_1(object sender, EventArgs e)
        {

        }

        private void pbClients_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox16_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click_1(object sender, EventArgs e)
        {
            MainGroupBox.Controls.Clear();
            var staffList = new StaffListAdmin();
            staffList.Dock = DockStyle.Fill;
            MainGroupBox.Controls.Add(staffList);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainGroupBox.Controls.Clear(); // remove old content

            StudentMainPrin studentView = new StudentMainPrin();
            studentView.Dock = DockStyle.Fill;

            MainGroupBox.Controls.Add(studentView);
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {

        }

        private void pbOrder_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            RevenueForm expenseTracker = new RevenueForm();
            expenseTracker.Show();
        }

        private void DashboardMenuBtn_Click(object sender, EventArgs e)
        {
            SiticoneGradientButton clickedButton = sender as SiticoneGradientButton;

            foreach (SiticoneGradientButton btn in sideex.Controls.OfType<SiticoneGradientButton>())
            {
                if (btn == clickedButton)
                {
                    btn.FillColor2 = Color.FromArgb(108, 92, 231);
                }
                else
                {
                    btn.FillColor2 = Color.Transparent;
                }
            }
            if (DashboardGroupBox.Visible == false)
            {
                DashboardGroupBox.Visible = true;
            }
            CreateDashboardButtons();
        }

        private void logoutbtn_Click_1(object sender, EventArgs e)
        {
            // Clear logged-in info
            LoggedInStaff.Username = null;
            LoggedInStaff.ManagesClass = null;

            // Return to login form
            LoginForm login = new LoginForm();
            login.Show();
            this.Close(); // Close main form
        }
    }
}
