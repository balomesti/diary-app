using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using diary_app.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace diary_app.Services
{
    public class DiaryService
    {
        private readonly HttpClient _http;

        public DiaryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DiaryEntry>> GetEntriesAsync()
        {
            try
            {
                var entries = await _http.GetFromJsonAsync<List<DiaryEntry>>("api/Diary");
                return entries ?? new List<DiaryEntry>();
            }
            catch
            {
                return new List<DiaryEntry>();
            }
        }

        public async Task<bool> SaveEntryAsync(DiaryEntry entry, IBrowserFile? imageFile)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(entry.Title), "Title");
            content.Add(new StringContent(entry.Content), "Content");
            content.Add(new StringContent(entry.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "Date");

            if (imageFile != null)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10)); // 10MB max
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(fileContent, "Image", imageFile.Name);
            }

            var response = await _http.PostAsync("api/Diary", content);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteEntryAsync(DiaryEntry entry)
        {
            var response = await _http.DeleteAsync($"api/Diary/{entry.Id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
