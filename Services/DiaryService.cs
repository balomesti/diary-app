using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Text.Json;
using diary_app.Models;

namespace diary_app.Services
{
    public class DiaryService
    {
        private readonly IJSRuntime _js;

        public DiaryService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<List<DiaryEntry>> GetEntriesAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", "diaryEntries");
            if (string.IsNullOrEmpty(json))
                return new List<DiaryEntry>();
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
        }

        public async Task SaveEntryAsync(DiaryEntry entry)
        {
            var entries = await GetEntriesAsync();
            entries.Add(entry);
            var json = JsonSerializer.Serialize(entries);
            await _js.InvokeVoidAsync("localStorage.setItem", "diaryEntries", json);
        }

        public async Task UpdateEntryAsync(DiaryEntry updatedEntry)
        {
            var entries = await GetEntriesAsync();
            var index = entries.FindIndex(e => e.Date == updatedEntry.Date && e.Title == updatedEntry.Title);
            if (index != -1)
            {
                entries[index] = updatedEntry;
                var json = JsonSerializer.Serialize(entries);
                await _js.InvokeVoidAsync("localStorage.setItem", "diaryEntries", json);
            }
        }

        public async Task DeleteEntryAsync(DiaryEntry entry)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Date == entry.Date && e.Title == entry.Title);
            var json = JsonSerializer.Serialize(entries);
            await _js.InvokeVoidAsync("localStorage.setItem", "diaryEntries", json);
        }
    }
}
