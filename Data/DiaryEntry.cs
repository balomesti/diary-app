namespace diary_app.Data;

public class DiaryEntry
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}