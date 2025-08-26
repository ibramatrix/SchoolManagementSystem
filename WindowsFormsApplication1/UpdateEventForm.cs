using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class UpdateEventForm : Form
    {
        private string firebaseKey, year, month;

        public UpdateEventForm(string key, string year, string month, string title, string date, string time, string status)
        {
            InitializeComponent();
            this.firebaseKey = key;
            this.year = year;
            this.month = month;

            titleTxt.Text = title;
            datePicker.Value = DateTime.TryParse(date, out var d) ? d : DateTime.Now;
            timeTxt.Text = time;
            statusCombo.SelectedItem = status;
        }

        private async void updateBtn_Click(object sender, EventArgs e)
        {
            // Prepare updated fields
            var updatedFields = new Dictionary<string, object>
    {
        { "Title", titleTxt.Text.Trim() },
        { "Date", datePicker.Value.ToString("MMM dd") },
        { "Time", timeTxt.Text.Trim() },
        { "Status", statusCombo.SelectedItem?.ToString() ?? "Upcoming" },
        { "LastUpdated", DateTime.UtcNow.ToString("o") }
    };

            try
            {
                await FirebaseHelper.PatchEvent(firebaseKey, year, month, updatedFields);
                MessageBox.Show("✅ Event updated successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Failed to update event:\n" + ex.Message);
            }
        }

    }
}
