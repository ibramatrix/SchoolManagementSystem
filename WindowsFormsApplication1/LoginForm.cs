using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Siticone.Desktop.UI.WinForms;

namespace WindowsFormsApplication1
{
    public partial class LoginForm : Form
    {
        private SiticoneTextBox usernameBox;
        private SiticoneTextBox passwordBox;
        private SiticoneButton loginBtn;
        private SiticoneHtmlLabel titleLabel;
        private LinkLabel registerLink;

        public LoginForm()
        {
            InitializeComponent();
            InitializeLoginUI();
        }

        private void InitializeLoginUI()
        {
            this.Text = "User Login";
            this.Size = new Size(480, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None; 

            // Gradient panel background
            var gradientPanel = new SiticoneGradientPanel()
            {
                Dock = DockStyle.Fill,
                FillColor = Color.FromArgb(74, 144, 226),
                FillColor2 = Color.FromArgb(142, 68, 173),
                BorderRadius = 20,
                BackColor = Color.Transparent
            };
            this.Controls.Add(gradientPanel);

            // Title
            titleLabel = new SiticoneHtmlLabel()
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(100, 40),
                BackColor = Color.Transparent
            };
            gradientPanel.Controls.Add(titleLabel);

            // Username box
            usernameBox = new SiticoneTextBox()
            {
                PlaceholderText = "Enter Username",
                Size = new Size(300, 45),
                Location = new Point(60, 130),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 11),
                IconLeft = Properties.Resources.user, // add user icon resource
            };
            gradientPanel.Controls.Add(usernameBox);

            // Password box
            passwordBox = new SiticoneTextBox()
            {
                PlaceholderText = "Enter Password",
                Size = new Size(300, 45),
                Location = new Point(60, 200),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 11),
                IconLeft = Properties.Resources._lock, // add lock icon resource
                UseSystemPasswordChar = true
            };
            gradientPanel.Controls.Add(passwordBox);

            // Login button
            loginBtn = new SiticoneButton()
            {
                Text = "Login",
                Size = new Size(300, 45),
                Location = new Point(60, 280),
                BorderRadius = 12,
                FillColor = Color.FromArgb(0, 123, 255),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };
            loginBtn.Click += async (s, e) => await LoginBtn_Click();
            gradientPanel.Controls.Add(loginBtn);

           
            var closeBtn = new SiticoneControlBox()
            {
                Location = new Point(this.Width - 50, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            gradientPanel.Controls.Add(closeBtn);
        }

        private async Task LoginBtn_Click()
        {
            string email = usernameBox.Text.Trim();
            string password = passwordBox.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("⚠️ Please enter both username and password.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = await LoginUser(email, password);
            if (result == "success")
            {
                MessageBox.Show("✅ Login successful!");
                this.Hide();
                Form1 main = new Form1();
                main.Show();
            }
            else if (result == "inactive")
            {
                MessageBox.Show("🚫 You're not allowed by the admin. Access denied.", "Account Inactive", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("❌ Wrong username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> LoginUser(string email, string password)
        {
            try
            {
                string url = "YOUR_API_KEY_HERE";

                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    var staffList = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                    foreach (var staffEntry in staffList)
                    {
                        var staff = staffEntry.Value;

                        if (staff.TryGetValue("Username", out object dbUsernameObj) &&
                            staff.TryGetValue("Password", out object dbPasswordObj) &&
                            staff.TryGetValue("isActive", out object isActiveObj))
                        {
                            string dbUsername = dbUsernameObj?.ToString();
                            string dbPassword = dbPasswordObj?.ToString();
                            bool isActive = Convert.ToBoolean(isActiveObj);

                            if (dbUsername == email && dbPassword == password)
                            {
                                if (!isActive)
                                {
                                    return "inactive";
                                }

                                LoggedInStaff.Username = dbUsername;
                                if (staff.TryGetValue("Manages", out object managesObj))
                                    LoggedInStaff.ManagesClass = managesObj?.ToString();

                                return "success";
                            }
                        }
                    }

                    return "fail"; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("🔥 Error during login:\n" + ex.Message);
                return "fail";
            }
        }
    }
}
