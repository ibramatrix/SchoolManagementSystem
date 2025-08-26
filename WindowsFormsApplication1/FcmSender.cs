using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WindowsFormsApplication1
{
    public static class FcmSender
    {
        private static string _serviceAccountPath = "fcm_service_account.json"; // Path to your JSON

        public static async Task SendNotificationAsync(string deviceToken, string title, string body)
        {
            try
            {
                // Load service account credentials
                GoogleCredential credential;
                using (var stream = new FileStream(_serviceAccountPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                }

                // Get OAuth2 access token
                var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                // Build FCM message payload
                var message = new
                {
                    message = new
                    {
                        token = deviceToken,
                        notification = new // <-- important for system notifications
                        {
                            title = title,
                            body = body,
                        },
                        android = new
                        {
                            priority = "high",
                            notification = new
                            {
                                channel_id = "test_reports_channel",
                                title = title,
                                body = body,
                                sound = "default",
                                default_vibrate_timings = true,
                                default_light_settings = true
                            }
                        },
                        apns = new
                        {
                            headers = new Dictionary<string, string> { { "apns-priority", "10" } },
                            payload = new
                            {
                                aps = new
                                {
                                    alert = new
                                    {
                                        title = title,
                                        body = body
                                    },
                                    sound = "default"
                                }
                            }
                        }
                    }
                };


                string jsonMessage = JsonConvert.SerializeObject(message);

                string url = "YOUR_API_KEY_HERE";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                        Console.WriteLine($"✅ Notification sent successfully! Response: {responseBody}");
                    else
                        Console.WriteLine($"❌ Failed to send notification. Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Exception while sending FCM notification: {ex.Message}");
            }
        }
    }
}
