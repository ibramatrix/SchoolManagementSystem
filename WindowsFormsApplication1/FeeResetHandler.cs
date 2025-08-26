using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsFormsApplication1
{
    public class FeeResetHandler
    {
        private const string FirebaseBackupUrl = "YOUR_API_KEY_HERE{0}.json";

        public async Task CheckAndResetFeeForNewYearAsync()
        {
            int year = DateTime.Now.Year;
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Only trigger on Jan 1st
            if (DateTime.Now.Month != 1 || DateTime.Now.Day != 1)
                return;

            string backupFile = $"Students_Backup_{year - 1}.json";
            string mainFile = "Students.json";

            if (!File.Exists(mainFile)) return;

            // Step 1: Load current students
            var json = File.ReadAllText(mainFile);
            var allClasses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Student>>>(json);

            // Step 2: Backup existing
            File.Copy(mainFile, backupFile, overwrite: true);

            // Optional: upload to Firebase or server
            await UploadBackupToFirebaseAsync(backupFile, year.ToString());

            // Step 3: Reset FeeStatus for new year
            foreach (var classEntry in allClasses.Values)
            {
                foreach (var student in classEntry.Values)
                {
                    var newStatus = new Dictionary<string, bool>();
                    for (int m = 1; m <= 12; m++)
                    {
                        string month = $"{year}-{m.ToString("D2")}";
                        newStatus[month] = false;
                    }
                    student.FeeStatus = newStatus;
                }
            }

            // Step 4: Save updated file
            File.WriteAllText(mainFile, JsonConvert.SerializeObject(allClasses, Formatting.Indented));

            MessageBox.Show($"✅ Fee status reset for new year {year}.\nBackup saved as {backupFile}.");
        }

        private async Task UploadBackupToFirebaseAsync(string filePath, string year)
        {
            if (!File.Exists(filePath)) return;

            string firebaseUrl = string.Format(FirebaseBackupUrl, year);
            string content = File.ReadAllText(filePath);

            using (var client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(new
                {
                    timestamp = DateTime.UtcNow.ToString("s"),
                    backup = content
                }), Encoding.UTF8, "application/json");

                var response = await client.PutAsync(firebaseUrl, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Uploaded backup to Firebase for {year}");
                }
                else
                {
                    Console.WriteLine("❌ Failed to upload backup to Firebase.");
                }
            }
        }
    }
}
