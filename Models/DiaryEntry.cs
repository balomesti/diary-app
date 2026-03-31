using System;
using System.ComponentModel.DataAnnotations;

namespace diary_app.Models
{
    public class DiaryEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Comment is required")]
        public string Content { get; set; } = string.Empty;
        public string ImageUrls { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Weather { get; set; } = string.Empty;
        public string Mood { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }
}
