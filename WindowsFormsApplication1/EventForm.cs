using Siticone.Desktop.UI.WinForms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class EventForm : Form
    {
        private SiticoneTextBox eventTitleTxt;
        private SiticoneTextBox eventTimeTxt;
        private SiticoneComboBox statusCombo;
        private SiticoneDateTimePicker eventDatePicker;
        private SiticoneComboBox eventTypeCombo; // StaffEvents or StudentEvents
        private SiticoneButton addEventBtn;

        public EventForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Add New Event";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Title Label and TextBox
            var lblTitle = new SiticoneHtmlLabel()
            {
                Text = "Event Title:",
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            eventTitleTxt = new SiticoneTextBox()
            {
                Location = new Point(20, 45),
                Size = new Size(340, 35),
                PlaceholderText = "Enter event title",
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };
            this.Controls.Add(eventTitleTxt);

            // Time Label and TextBox
            var lblTime = new SiticoneHtmlLabel()
            {
                Text = "Time (e.g. 11:00 AM):",
                ForeColor = Color.White,
                Location = new Point(20, 90),
                AutoSize = true
            };
            this.Controls.Add(lblTime);

            eventTimeTxt = new SiticoneTextBox()
            {
                Location = new Point(20, 115),
                Size = new Size(200, 35),
                PlaceholderText = "Enter event time",
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };
            this.Controls.Add(eventTimeTxt);

            // Status Label and ComboBox
            var lblStatus = new SiticoneHtmlLabel()
            {
                Text = "Status:",
                ForeColor = Color.White,
                Location = new Point(230, 90),
                AutoSize = true
            };
            this.Controls.Add(lblStatus);

            statusCombo = new SiticoneComboBox()
            {
                Location = new Point(230, 115),
                Size = new Size(130, 35),
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusCombo.Items.Add("Upcoming");
            statusCombo.Items.Add("Cancelled");
            statusCombo.SelectedIndex = 0;
            this.Controls.Add(statusCombo);

            // Date Label and DateTimePicker
            var lblDate = new SiticoneHtmlLabel()
            {
                Text = "Date:",
                ForeColor = Color.White,
                Location = new Point(20, 160),
                AutoSize = true
            };
            this.Controls.Add(lblDate);

            eventDatePicker = new SiticoneDateTimePicker()
            {
                Location = new Point(20, 185),
                Size = new Size(340, 35),
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Format = DateTimePickerFormat.Long
            };
            this.Controls.Add(eventDatePicker);

            // Event Type Label and ComboBox
            var lblEventType = new SiticoneHtmlLabel()
            {
                Text = "Event Type:",
                ForeColor = Color.White,
                Location = new Point(20, 230),
                AutoSize = true
            };
            this.Controls.Add(lblEventType);

            eventTypeCombo = new SiticoneComboBox()
            {
                Location = new Point(20, 255),
                Size = new Size(340, 35),
                FillColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            eventTypeCombo.Items.Add("StaffEvents");
            eventTypeCombo.Items.Add("StudentEvents");
            eventTypeCombo.SelectedIndex = 0;
            this.Controls.Add(eventTypeCombo);

            // Add Event Button
            addEventBtn = new SiticoneButton()
            {
                Text = "Add Event",
                Location = new Point(20, 310),
                Size = new Size(340, 45),
                BorderRadius = 10,
                FillColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            addEventBtn.Click += AddEventBtn_Click;
            this.Controls.Add(addEventBtn);
        }

        private async void AddEventBtn_Click(object sender, EventArgs e)
        {
            string title = eventTitleTxt.Text.Trim();
            string time = eventTimeTxt.Text.Trim();
            string status = statusCombo.SelectedItem?.ToString() ?? "Upcoming";
            DateTime date = eventDatePicker.Value;
            string eventType = eventTypeCombo.SelectedItem?.ToString() ?? "StaffEvents";

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter the event title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(time))
            {
                MessageBox.Show("Please enter the event time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string year = date.Year.ToString();
            string month = date.ToString("MMMM");
            string formattedDate = date.ToString("MMM dd");
            string lastUpdated = DateTime.UtcNow.ToString("o");

            var newEvent = new SchoolEvent
            {
                Title = title,
                Time = time,
                Date = formattedDate,
                Status = status,
                LastUpdated = lastUpdated
            };

            try
            {
                string firebaseKey = await FirebaseHelper.UploadEventToFirebase(eventType, year, month, newEvent);

                MessageBox.Show("Event added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading event: " + ex.Message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

   
}
