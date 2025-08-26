using CefSharp.DevTools.IO;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsFormsApplication1;
using Firebase.Storage;
using Google.Cloud.Storage.V1;

public static class FirebaseHelper
{
    private static readonly string firebaseUrl = "YOUR_API_KEY_HERE"; // change this
    private static readonly FirebaseClient firebaseClient = new FirebaseClient(firebaseUrl);
    private static readonly string firebaseStorageBucket = "YOUR_API_KEY_HERE"; // your bucket

    private static Google.Cloud.Storage.V1.StorageClient storageClient;

    public static async Task UploadRevenueToFirebase(string monthKey, RevenueEntry entry)
    {
        await firebaseClient
            .Child("Revenue")
            .Child(monthKey)
            .PutAsync(entry);
    }

    public static async Task UploadStaffToFirebase(string staffId, Staff staff)
    {
        await firebaseClient
            .Child("Stafflist")
            .Child(staffId)
            .PutAsync(staff);
    }

    public static async Task<string> UploadEventToFirebase(string eventType, string year, string month, SchoolEvent newEvent)
    {
        using (var client = new HttpClient())
        {
            string baseUrl = "YOUR_API_KEY_HERE";

            string url = $"{baseUrl}/{eventType}/{year}/{month}.json";

            var json = JsonConvert.SerializeObject(newEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            // Parse the response to get the generated key
            var keyObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
            if (keyObj != null && keyObj.ContainsKey("name"))
            {
                return keyObj["name"];
            }
            return null;
        }
    }


    public static async Task<List<SchoolEvent>> GetEventsForMonth(string year, string month)
    {
        var events = await firebaseClient
            .Child("EventList")
            .Child(year)
            .Child(month)
            .OnceAsync<SchoolEvent>();

        List<SchoolEvent> result = new List<SchoolEvent>();
        foreach (var item in events)
        {
            result.Add(item.Object);
        }

        return result;
    }

    public static async Task UploadDataAsync(string path, object data)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(data);
                string url = $"{firebaseUrl}{path}.json"; // Firebase REST API

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"❌ Failed to upload: {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Firebase upload error: {ex.Message}");
        }
    }

    public static async Task<object> GetDataAsync(string path)
    {
        try
        {
            string url = $"{firebaseUrl}{path}.json"; // <- change baseUrl to firebaseUrl
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Firebase GET failed for path {path}. Status: {response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json) || json == "null") return null;

                // Deserialize JSON (returns string for FCM token)
                var value = JsonConvert.DeserializeObject<object>(json);
                return value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Exception while fetching Firebase data: {ex.Message}");
            return null;
        }
    }


    public static async Task<Dictionary<string, Dictionary<string, Student>>> LoadClassData()
    {
        try
        {
            var studentsNode = await firebaseClient
                .Child("students")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, Student>>>();

            return studentsNode ?? new Dictionary<string, Dictionary<string, Student>>();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error fetching data: " + ex.Message);
            return new Dictionary<string, Dictionary<string, Student>>();
        }
    }

    public static async Task UpdateEvent(string key, string year, string month, SchoolEvent updatedEvent)
    {
        await firebaseClient
            .Child("EventList")
            .Child(year)
            .Child(month)
            .Child(key)
            .PutAsync(updatedEvent);
    }

    public static async Task SaveNoteAsync(string className, string noteTitle, string filePath)
    {
        string fileUrl = await UploadFileToStorage(className, filePath);

        var note = new { Title = noteTitle, FileUrl = fileUrl };

        await firebaseClient
            .Child("Notes")
            .Child(className)
            .Child(noteTitle)
            .PutAsync(note);
    }

    public static async Task<Dictionary<string, dynamic>> GetNotesAsync(string className)
    {
        var notes = await firebaseClient
            .Child("Notes")
            .Child(className)
            .OnceAsync<dynamic>();

        Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
        foreach (var n in notes)
        {
            result.Add(n.Key, n.Object);
        }

        return result;
    }

    public static async Task DeleteNoteAsync(string className, string noteTitle)
    {
        await firebaseClient
            .Child("Notes")
            .Child(className)
            .Child(noteTitle)
            .DeleteAsync();
    }

    public static async Task UpdateNoteAsync(string className, string noteTitle, string newFilePath)
    {
        string fileUrl = await UploadFileToStorage(className, newFilePath);
        var note = new { Title = noteTitle, FileUrl = fileUrl };
        await firebaseClient
            .Child("Notes")
            .Child(className)
            .Child(noteTitle)
            .PutAsync(note);
    }

    public static async Task<string> UploadFileToStorage(string className, string filePath)
    {
        InitializeStorageClient();

        string objectName = $"Notes/{className}/{Guid.NewGuid()}{Path.GetExtension(filePath)}";
        string downloadToken = Guid.NewGuid().ToString();

        var obj = new Google.Apis.Storage.v1.Data.Object()
        {
            Bucket = firebaseStorageBucket,
            Name = objectName,
            Metadata = new Dictionary<string, string>()
            {
                { "firebaseStorageDownloadTokens", downloadToken }
            }
        };

        using (var stream = File.OpenRead(filePath))
        {
            await storageClient.UploadObjectAsync(obj, stream);
        }

        string downloadUrl = $"YOUR_API_KEY_HERE{firebaseStorageBucket}/o/{Uri.EscapeDataString(objectName)}?alt=media&token={downloadToken}";
        return downloadUrl;
    }

    private static void InitializeStorageClient()
    {
        if (storageClient == null)
        {
            string serviceAccountJsonPath = @"API-KEY";
            storageClient = StorageClient.Create(
                Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(serviceAccountJsonPath)
            );
        }
    }

    public static async Task PatchEvent(string key, string year, string month, Dictionary<string, object> updatedFields)
    {
        string url = $"YOUR_API_KEY_HERE{year}/{month}/{key}.json";

        using (var client = new HttpClient())
        {
            var json = JsonConvert.SerializeObject(updatedFields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = content
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }

}
