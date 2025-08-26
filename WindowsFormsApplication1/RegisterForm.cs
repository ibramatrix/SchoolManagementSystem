using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApplication1
{
    public partial class RegisterForm : Form
    {
        private TextBox nameBox, emailBox, passwordBox, confirmPasswordBox;
        private Button registerBtn;

        public RegisterForm()
        {
            InitializeComponent();
            InitializeRegisterUI();
        }

        private void InitializeRegisterUI()
        {
            this.Text = "Register";
            this.Size = new Size(450, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label titleLabel = new Label()
            {
                Text = "Create New Account",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(80, 30),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(titleLabel);

            Label nameLabel = new Label() { Text = "Name", Location = new Point(50, 100), Font = new Font("Segoe UI", 11) };
            nameBox = new TextBox() { Location = new Point(50, 130), Width = 320, Font = new Font("Segoe UI", 11) };

            Label emailLabel = new Label() { Text = "Email", Location = new Point(50, 180), Font = new Font("Segoe UI", 11) };
            emailBox = new TextBox() { Location = new Point(50, 210), Width = 320, Font = new Font("Segoe UI", 11) };

            Label passLabel = new Label() { Text = "Password", Location = new Point(50, 260), Font = new Font("Segoe UI", 11) };
            passwordBox = new TextBox()
            {
                Location = new Point(50, 290),
                Width = 320,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true
            };

            Label confirmPassLabel = new Label() { Text = "Confirm Password", Location = new Point(50, 340), Font = new Font("Segoe UI", 11) };
            confirmPasswordBox = new TextBox()
            {
                Location = new Point(50, 370),
                Width = 320,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true
            };

            registerBtn = new Button()
            {
                Text = "Register",
                Location = new Point(50, 430),
                Width = 320,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            registerBtn.Click += async (s, e) => await RegisterBtn_Click();

            this.Controls.AddRange(new Control[]
            {
                nameLabel, nameBox,
                emailLabel, emailBox,
                passLabel, passwordBox,
                confirmPassLabel, confirmPasswordBox,
                registerBtn
            });
        }

        private async Task RegisterBtn_Click()
        {
            string name = nameBox.Text.Trim();
            string email = emailBox.Text.Trim();
            string password = passwordBox.Text;
            string confirmPassword = confirmPasswordBox.Text;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("All fields are required.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            bool success = await RegisterUser(email, password);
            if (success)
            {
                MessageBox.Show("Registration successful!");
                this.Close();
            }
        }

        private async Task<bool> RegisterUser(string email, string password)
        {
            string apiKey = "YOUR_API_KEY_HERE";  // ✅ Your working API key
            string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

            var client = new HttpClient();
            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                dynamic err = JsonConvert.DeserializeObject(result);
                string errorMessage = err?.error?.message ?? "Unknown error";
                MessageBox.Show("Registration failed: " + errorMessage);
                return false;
            }
        }


    }
}
