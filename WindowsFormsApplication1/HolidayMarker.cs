using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class HolidayMarker : Form
    {
        private FirebaseClient firebase;

        // UI Controls
        private SiticoneDateTimePicker datePicker;
        private SiticoneTextBox holidayNameTxt;
        private SiticoneComboBox holidayTypeCombo;
        private SiticoneButton addBtn, updateBtn, deleteBtn;
        private SiticoneDataGridView holidaysGrid;

        private string selectedDateKey = null;

        public HolidayMarker()
        {
            InitializeComponent();
            InitializeFirebase();
            SetupUI();
            LoadHolidays();
        }

        private void InitializeFirebase()
        {
            firebase = new FirebaseClient("YOUR_API_KEY_HERE");
        }

        private void SetupUI()
        {
            this.Text = "Holiday Marker";
            this.Size = new System.Drawing.Size(700, 500);
            this.BackColor = System.Drawing.Color.White;

            datePicker = new SiticoneDateTimePicker
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 200,
                MinDate = DateTime.Today
            };
            this.Controls.Add(datePicker);

            holidayNameTxt = new SiticoneTextBox
            {
                PlaceholderText = "Enter Holiday Name",
                Location = new System.Drawing.Point(240, 20),
                Width = 200
            };
            this.Controls.Add(holidayNameTxt);

            holidayTypeCombo = new SiticoneComboBox
            {
                Location = new System.Drawing.Point(460, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            holidayTypeCombo.Items.AddRange(new object[] { "National", "International", "Religious" });
            this.Controls.Add(holidayTypeCombo);

            addBtn = new SiticoneButton
            {
                Text = "Add Holiday",
                Location = new System.Drawing.Point(20, 60),
                Width = 150
            };
            addBtn.Click += async (s, e) => await AddHoliday();
            this.Controls.Add(addBtn);

            updateBtn = new SiticoneButton
            {
                Text = "Update Holiday",
                Location = new System.Drawing.Point(180, 60),
                Width = 150
            };
            updateBtn.Click += async (s, e) => await UpdateHoliday();
            this.Controls.Add(updateBtn);

            deleteBtn = new SiticoneButton
            {
                Text = "Delete Holiday",
                Location = new System.Drawing.Point(340, 60),
                Width = 150
            };
            deleteBtn.Click += async (s, e) => await DeleteHoliday();
            this.Controls.Add(deleteBtn);

            holidaysGrid = new SiticoneDataGridView
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(640, 340),
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            holidaysGrid.CellClick += HolidaysGrid_CellClick;
            this.Controls.Add(holidaysGrid);
        }

        private async void LoadHolidays()
        {
            var holidays = await firebase
                .Child("Holidays")
                .OnceAsync<Dictionary<string, string>>();

            holidaysGrid.Rows.Clear();
            holidaysGrid.Columns.Clear();
            holidaysGrid.Columns.Add("Date", "Date");
            holidaysGrid.Columns.Add("Name", "Holiday Name");
            holidaysGrid.Columns.Add("Type", "Type");

            foreach (var h in holidays)
            {
                var date = h.Key;
                var holidayData = await firebase.Child("Holidays").Child(date).OnceSingleAsync<HolidayData>();
                holidaysGrid.Rows.Add(date, holidayData.name, holidayData.type);
            }
        }

        private async Task AddHoliday()
        {
            var dateKey = datePicker.Value.ToString("yyyy-MM-dd");
            var name = holidayNameTxt.Text.Trim();
            var type = holidayTypeCombo.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            await firebase.Child("Holidays").Child(dateKey).PutAsync(new HolidayData
            {
                name = name,
                type = type
            });

            MessageBox.Show("Holiday added!");
            LoadHolidays();
        }

        private async Task UpdateHoliday()
        {
            if (selectedDateKey == null)
            {
                MessageBox.Show("Select a holiday from the list to update.");
                return;
            }

            var name = holidayNameTxt.Text.Trim();
            var type = holidayTypeCombo.SelectedItem?.ToString();

            await firebase.Child("Holidays").Child(selectedDateKey).PutAsync(new HolidayData
            {
                name = name,
                type = type
            });

            MessageBox.Show("Holiday updated!");
            LoadHolidays();
        }

        private async Task DeleteHoliday()
        {
            if (selectedDateKey == null)
            {
                MessageBox.Show("Select a holiday from the list to delete.");
                return;
            }

            await firebase.Child("Holidays").Child(selectedDateKey).DeleteAsync();

            MessageBox.Show("Holiday deleted!");
            LoadHolidays();
        }

        private void HolidaysGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedDateKey = holidaysGrid.Rows[e.RowIndex].Cells[0].Value?.ToString();
                holidayNameTxt.Text = holidaysGrid.Rows[e.RowIndex].Cells[1].Value?.ToString();
                holidayTypeCombo.SelectedItem = holidaysGrid.Rows[e.RowIndex].Cells[2].Value?.ToString();
                datePicker.Value = DateTime.Parse(selectedDateKey);
            }
        }
    }

    public class HolidayData
    {
        public string name { get; set; }
        public string type { get; set; }
    }
}
