// Required namespaces
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Interop;

namespace WindowsFormsApplication1
{
    public partial class FeeReminder : Form
    {
        private ComboBox classComboBox;
        private DataGridView dgv;
        private Button sendAllButton, printBtn;
        private Dictionary<string, Dictionary<string, Student>> allClasses;
        private readonly string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToArray();

        public FeeReminder()
        {
            InitializeComponent();
            LoadStudentData();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "💸 Fee Reminder";
            this.Size = new Size(1500, 900);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // Add title label at the top
            Label title = new Label
            {
                Text = "📚 Universal School Fee Reminder Dashboard",
                Font = new System.Drawing.Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = false,
                Size = new Size(this.Width, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 10)
            };
            this.Controls.Add(title);

            int controlTop = title.Bottom + 10;

            classComboBox = new ComboBox
            {
                Location = new Point(30, controlTop),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Segoe UI", 10),
                BackColor = Color.White
            };
            classComboBox.SelectedIndexChanged += (s, e) => LoadClassStudents();

            sendAllButton = CreateStyledButton("📢 Send Msgs to Unpaid", 300, controlTop, Color.FromArgb(52, 152, 219));
            sendAllButton.Click += SendAllUnpaidMessages;

            printBtn = CreateStyledButton("🖨 Print Fee Voucher", 530, controlTop, Color.FromArgb(231, 76, 60));
           

            // Add controls to form
            this.Controls.Add(classComboBox);
            this.Controls.Add(sendAllButton);
            this.Controls.Add(printBtn);

            dgv = new DataGridView
            {
                Location = new Point(30, sendAllButton.Bottom + 20),
                Width = 1420,
                Height = 700,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 35 },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 245, 245)
                }
            };

            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            this.Controls.Add(dgv);

            if (allClasses != null)
            {
                classComboBox.Items.AddRange(allClasses.Keys.ToArray());
                if (classComboBox.Items.Count > 0)
                    classComboBox.SelectedIndex = 0;
            }
        }



        private Button CreateStyledButton(string text, int x, int y, Color bgColor)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(200, 35),
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = bgColor,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }


        private void LoadStudentData()
        {
            if (File.Exists("Students.json"))
            {
                var json = File.ReadAllText("Students.json");
                allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);
            }
        }

        private void SaveStudentData()
        {
            File.WriteAllText("Students.json", JsonConvert.SerializeObject(allClasses, Formatting.Indented));
        }

        private void LoadClassStudents()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            string selectedClass = classComboBox.SelectedItem.ToString();
            var students = allClasses[selectedClass];

            dgv.Columns.Add("ID", "Student ID");
            dgv.Columns["ID"].Width = 100;

            dgv.Columns.Add("Name", "Student Name");
            dgv.Columns["Name"].Width = 220;  // ✅ Wide for full names

            dgv.Columns.Add("Contact", "Contact");
            dgv.Columns["Contact"].Width = 150;

            foreach (var month in months)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    HeaderText = month,
                    Name = month,
                    Width = 80
                };
                dgv.Columns.Add(col);
            }

            // Add buttons
            DataGridViewButtonColumn msgBtn = new DataGridViewButtonColumn
            {
                Name = "Send",
                Text = "Send",
                UseColumnTextForButtonValue = true,
                Width = 100
            };
            DataGridViewButtonColumn prtBtn = new DataGridViewButtonColumn
            {
                Name = "Print",
                Text = "Print",
                UseColumnTextForButtonValue = true,
                Width = 100
            };

            dgv.Columns.Add(msgBtn);
            dgv.Columns.Add(prtBtn);

            foreach (var s in students)
            {
                var student = s.Value;
                var values = new List<object> { s.Key, student.Name, student.Contact };

                foreach (var month in months)
                {
                    if (student.FeeStatus == null)
                        student.FeeStatus = new Dictionary<string, bool>();

                    bool paid = student.FeeStatus.ContainsKey(month) && student.FeeStatus[month];
                    values.Add(paid ? "✅ Paid" : "❌ Not Paid");
                }

                int rowIndex = dgv.Rows.Add(values.ToArray());

                // Coloring logic
                for (int i = 0; i < months.Length; i++)
                {
                    int columnIndex = i + 3;
                    if (!student.FeeStatus.ContainsKey(months[i]) || !student.FeeStatus[months[i]])
                    {
                        if (DateTime.Now.Month == i + 1 && DateTime.Now.Day > 10)
                            dgv.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.LightPink;
                    }
                }
            }

            dgv.CellContentClick += dgv_CellContentClick;
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 3 || e.ColumnIndex >= 3 + months.Length) return;

            var row = dgv.Rows[e.RowIndex];
            string studentId = row.Cells["ID"].Value.ToString();
            string selectedClass = classComboBox.SelectedItem.ToString();
            string month = dgv.Columns[e.ColumnIndex].HeaderText;

            var student = allClasses[selectedClass][studentId];
            if (student.FeeStatus == null)
                student.FeeStatus = new Dictionary<string, bool>();

            bool isPaid = student.FeeStatus.ContainsKey(month) && student.FeeStatus[month];
            student.FeeStatus[month] = !isPaid;

            SaveStudentData();
            LoadClassStudents();
        }

        private async void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string studentId = dgv.Rows[e.RowIndex].Cells["ID"].Value.ToString();
            string name = dgv.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            string contact = dgv.Rows[e.RowIndex].Cells["Contact"].Value.ToString();
            string selectedClass = classComboBox.SelectedItem.ToString();

            if (dgv.Columns[e.ColumnIndex].Name == "Send")
            {
                var unpaid = months
                    .Where(m => !(allClasses[selectedClass][studentId].FeeStatus.ContainsKey(m)
                                   && allClasses[selectedClass][studentId].FeeStatus[m]))
                    .ToList();

                if (unpaid.Count == 0)
                {
                    MessageBox.Show("All months are paid.");
                    return;
                }

                string message = $"Assalamualaikum {name}, please pay your fee for: {string.Join(", ", unpaid)}.";
                MessageBox.Show($"[TEST MODE]\n{message}\n(To: {contact})");

                string year = DateTime.Now.Year.ToString();
                var student = allClasses[selectedClass][studentId];

                foreach (var month in unpaid)
                {
                    var data = new
                    {
                        Name = name,
                        Contact = contact,
                        Message = message
                    };

                    string path = $"Reminder/FeesReminder/{selectedClass}/{year}/{month}/{studentId}";
                    await FirebaseHelper.UploadDataAsync(path, data);

                    string tokenPath = $"students/{selectedClass}/{studentId}/fcmToken";
                    var tokenFromFirebase = await FirebaseHelper.GetDataAsync(tokenPath) as string;

                    // ✅ Safe FCM send
                    if (!string.IsNullOrEmpty(tokenFromFirebase))
                    {
                        try
                        {
                            await FcmSender.SendNotificationAsync(
                                    deviceToken: tokenFromFirebase,
                            title: "Fee Reminder",
                                    body: message);
                            MessageBox.Show("✅ Behj Dia.");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Failed to send FCM to {name}: {ex.Message}");
                            // Continue without crashing
                        }
                    }
                }

                MessageBox.Show("✅ Reminder uploaded and FCM sent (if token exists).");
            }

            else if (dgv.Columns[e.ColumnIndex].Name == "Print")
            {
                var student = allClasses[selectedClass][studentId];


                // Get unpaid months
                var unpaidMonths = months
                    .Where(m => !student.FeeStatus.ContainsKey(m) || !student.FeeStatus[m])
                    .ToList();

                if (unpaidMonths.Count == 0)
                {
                    MessageBox.Show("✅ All months are already paid.");
                    return;
                }

                // Open the editable voucher form
                VoucherForm voucherForm = new VoucherForm(student, unpaidMonths, selectedClass);
                voucherForm.ShowDialog();
            }

        }

        private async void SendAllUnpaidMessages(object sender, EventArgs e)
        {
            string selectedClass = classComboBox.SelectedItem.ToString();
            string year = DateTime.Now.Year.ToString();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string studentId = row.Cells["ID"].Value.ToString();
                string name = row.Cells["Name"].Value.ToString();
                string contact = row.Cells["Contact"].Value.ToString();

                var student = allClasses[selectedClass][studentId];

                var unpaid = months
                    .Where(m => !student.FeeStatus.ContainsKey(m) || !student.FeeStatus[m])
                    .ToList();

                if (unpaid.Count > 0)
                {
                    string msg = $"Assalamualaikum {name}, please pay your fee for: {string.Join(", ", unpaid)}.";

                    foreach (var month in unpaid)
                    {
                        var data = new
                        {
                            Name = name,
                            Contact = contact,
                            Message = msg
                        };

                        string path = $"Reminder/FeesReminder/{selectedClass}/{year}/{month}/{studentId}";
                        await FirebaseHelper.UploadDataAsync(path, data);

                        // ✅ Fetch FCM token directly from Firebase
                        string tokenPath = $"students/{selectedClass}/{studentId}/fcmToken";
                        var tokenObj = await FirebaseHelper.GetDataAsync(tokenPath);
                        string fcmToken = tokenObj?.ToString();

                        if (!string.IsNullOrEmpty(fcmToken))
                        {
                            try
                            {
                                await FcmSender.SendNotificationAsync(
                                    deviceToken: fcmToken,
                                    title: "Fee Reminder",
                                    body: msg
                                );
                                Console.WriteLine($"✅ Notification successfully sent to {name}!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Failed to send FCM to {name}: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ No FCM token found for {name}, skipping notification.");
                        }
                    }

                    MessageBox.Show($"✅ Reminder uploaded and FCM sent (if token exists) for {name}");
                }
            }
        }


    }


}
