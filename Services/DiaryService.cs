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

        public async Task<DiaryEntry?> GetEntryByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<DiaryEntry>($"api/Diary/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveEntryAsync(DiaryEntry entry, List<IBrowserFile>? imageFiles)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(entry.Title), "Title");
            content.Add(new StringContent(entry.Content), "Content");
            content.Add(new StringContent(entry.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "Date");
            content.Add(new StringContent(entry.Location ?? ""), "Location");
            content.Add(new StringContent(entry.Weather ?? ""), "Weather");
            content.Add(new StringContent(entry.Mood ?? ""), "Mood");
            content.Add(new StringContent(entry.Tags ?? ""), "Tags");

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var imageFile in imageFiles)
                {
                    var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    content.Add(fileContent, "Images", imageFile.Name);
                }
            }

            var response = await _http.PostAsync("api/Diary", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateEntryAsync(DiaryEntry entry, List<IBrowserFile>? imageFiles)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(entry.Title), "Title");
            content.Add(new StringContent(entry.Content), "Content");
            content.Add(new StringContent(entry.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "Date");
            content.Add(new StringContent(entry.Location ?? ""), "Location");
            content.Add(new StringContent(entry.Weather ?? ""), "Weather");
            content.Add(new StringContent(entry.Mood ?? ""), "Mood");
            content.Add(new StringContent(entry.Tags ?? ""), "Tags");

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var imageFile in imageFiles)
                {
                    var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    content.Add(fileContent, "Images", imageFile.Name);
                }
            }

            var response = await _http.PutAsync($"api/Diary/{entry.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteEntryAsync(DiaryEntry entry)
        {
            var response = await _http.DeleteAsync($"api/Diary/{entry.Id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<KeyHighlight>> GetHighlightsAsync(int entryId)
        {
            var response = await _http.GetAsync($"api/Diary/{entryId}/highlights");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Load highlights failed ({(int)response.StatusCode}): {body}");
            }
            var items = await response.Content.ReadFromJsonAsync<List<KeyHighlight>>();
            return items ?? new List<KeyHighlight>();
        }

        public async Task<KeyHighlight> AddHighlightAsync(int entryId, KeyHighlight highlight)
        {
            var response = await _http.PostAsJsonAsync($"api/Diary/{entryId}/highlights", new
            {
                title = highlight.Title,
                description = highlight.Description,
                icon = highlight.Icon,
                time = highlight.Time
            });
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Save highlight failed ({(int)response.StatusCode}): {body}");
            }
            var created = await response.Content.ReadFromJsonAsync<KeyHighlight>();
            if (created == null)
            {
                throw new HttpRequestException("Save highlight failed: empty response.");
            }
            return created;
        }

        // Admin News and Announcements
        public async Task<bool> CreateNewsAsync(string title, string content, IBrowserFile? imageFile, IBrowserFile? videoFile = null)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(title ?? string.Empty), "Title");
            form.Add(new StringContent(content ?? string.Empty), "Content");

            if (imageFile != null)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10)); // 10MB max
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                form.Add(fileContent, "Image", imageFile.Name);
            }

            if (videoFile != null)
            {
                var fileContent = new StreamContent(videoFile.OpenReadStream(maxAllowedSize: 1024 * 1024 * 100)); // 100MB max for video
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(videoFile.ContentType);
                form.Add(fileContent, "Video", videoFile.Name);
            }

            var response = await _http.PostAsync("api/Admin/news", form);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Create news failed ({(int)response.StatusCode}): {body}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateAnnouncementAsync(object announcement)
        {
            var response = await _http.PostAsJsonAsync("api/Admin/announcements", announcement);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Create announcement failed ({(int)response.StatusCode}): {body}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteNewsAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Admin/news/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Delete news failed ({(int)response.StatusCode}): {body}");
            }
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Admin/announcements/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Delete announcement failed ({(int)response.StatusCode}): {body}");
            }
        }

        public async Task<List<T>> GetAdminListAsync<T>(string url)
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Load failed ({(int)response.StatusCode}): {body}");
            }
            var items = await response.Content.ReadFromJsonAsync<List<T>>();
            return items ?? new List<T>();
        }
    }
}
