using CefSharp;
using CefSharp.WinForms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class FescoBillBrowser : Form
    {
        private ChromiumWebBrowser browser;
        private string refNumber;

        public FescoBillBrowser(string referenceNumber)
        {
            InitializeComponent();
            refNumber = referenceNumber;
            InitializeBrowser();
        }

        private void InitializeBrowser()
        {
            this.Size = new Size(1000, 700);
            this.Text = "Loading FESCO Bill...";

            browser = new ChromiumWebBrowser("https://bill.pitc.com.pk/fescobill");
            browser.Dock = DockStyle.Fill;
            this.Controls.Add(browser);

            browser.FrameLoadEnd += async (sender, args) =>
            {
                if (args.Frame.IsMain)
                {
                    await Task.Delay(1500); // Wait a bit for the DOM to be ready

                    if (!browser.IsDisposed && browser.CanExecuteJavascriptInMainFrame)
                    {
                        try
                        {
                            await browser.EvaluateScriptAsync($"document.getElementById('searchTextBox').value = '{refNumber}';");
                            await Task.Delay(500);
                            await browser.EvaluateScriptAsync("document.getElementById('btnSearch').click();");

                            _ = WaitForResult(); // Start the result checker
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("JavaScript execution failed:\n" + ex.Message);
                        }
                    }
                }
            };
        }

        private async Task WaitForResult()
        {
            int tries = 0;
            const int maxTries = 30; // Wait for up to 30 seconds

            while (tries < maxTries)
            {
                await Task.Delay(1000);

                if (browser == null || browser.IsDisposed || !browser.CanExecuteJavascriptInMainFrame)
                    break;

                try
                {
                    var frame = browser.GetMainFrame();
                    var result = await frame.EvaluateScriptAsync(@"
                        (function() {
                            try {
                                var result = document.evaluate(
                                    '/html/body/div[3]/div[4]/div[4]/table/tbody/tr[1]/td[5]',
                                    document,
                                    null,
                                    XPathResult.FIRST_ORDERED_NODE_TYPE,
                                    null
                                );
                                return result.singleNodeValue ? result.singleNodeValue.innerText.trim() : null;
                            } catch(e) {
                                return null;
                            }
                        })();
                    ");

                    if (result.Success && result.Result != null)
                    {
                        string billText = result.Result.ToString().Split('\n')[0].Trim().Replace(",", "");
                        if (int.TryParse(billText, out int amount))
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.Tag = amount;
                                    this.DialogResult = DialogResult.OK;
                                    this.Close();
                                }));
                            }
                            else
                            {
                                this.Tag = amount;
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Evaluation Error: " + ex.Message);
                    break;
                }

                tries++;
            }

            if (!this.IsDisposed)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Bill not found in time.");
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }));
            }
        }
    }
}
