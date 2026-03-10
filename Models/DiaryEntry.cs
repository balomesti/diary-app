using System;

namespace diary_app.Models
{
    public class DiaryEntry
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
