namespace diary_app.Models
{
    public class KeyHighlight
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🌟";
        public string Time { get; set; } = "";
        public int DiaryEntryId { get; set; }
    }
}
