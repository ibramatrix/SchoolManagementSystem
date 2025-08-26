using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using PdfiumViewer;

namespace WindowsFormsApplication1
{
    public static class WhatsAppSender
    {
        public static void SendWhatsAppScreenshot(string pdfPath)
        {
            var result = MessageBox.Show(
                "Do you want to send the report card via WhatsApp (as a screenshot)?",
                "Send Report",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            // Load and take screenshot
            string imagePath = Path.Combine(Path.GetDirectoryName(pdfPath), "ReportScreenshot.png");

            try
            {
                using (var doc = PdfiumViewer.PdfDocument.Load(pdfPath))
                using (var img = doc.Render(0, 800, 1100, true)) // First page
                {
                    img.Save(imagePath, ImageFormat.Png);
                }

                // Copy image path to clipboard for convenience
                Clipboard.SetText(imagePath);

                // Open WhatsApp web
                string phone = "9999999";
                string message = "Here is your report card screenshot. Please see the attached image.";
                string url = $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                // Open the folder where image is saved so user can drag it
                Process.Start("explorer.exe", $"/select,\"{imagePath}\"");

                MessageBox.Show("WhatsApp opened in browser. Screenshot is saved and folder opened. Please attach it manually.", "Ready to Send", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to take screenshot: " + ex.Message);
            }
        }
    }
}
