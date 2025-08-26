using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class ProgressForm : Form
    {
        private Timer glowTimer;
        private int glowStep = 0;

        public ProgressForm()
        {
            InitializeComponent();
            SetupGlowEffect();
            ApplyRoundedCorners();
        }

        private void SetupGlowEffect()
        {
            glowTimer = new Timer();
            glowTimer.Interval = 100;
            glowTimer.Tick += GlowTimer_Tick;
            glowTimer.Start();
        }

        private void GlowTimer_Tick(object sender, EventArgs e)
        {
            glowStep = (glowStep + 1) % 360;
            int r = (int)(Math.Sin(glowStep * Math.PI / 180) * 30 + 150);
            int g = (int)(Math.Sin((glowStep + 120) * Math.PI / 180) * 30 + 150);
            int b = (int)(Math.Sin((glowStep + 240) * Math.PI / 180) * 30 + 150);

            this.BackColor = Color.FromArgb(r, g, b);
        }

        private void ApplyRoundedCorners()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}
