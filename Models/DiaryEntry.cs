using System;

namespace diary_app.Models
{
    public class DiaryEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrls { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Weather { get; set; } = string.Empty;
        public string Mood { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }
}
