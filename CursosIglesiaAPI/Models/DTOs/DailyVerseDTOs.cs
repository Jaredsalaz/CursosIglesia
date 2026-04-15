namespace CursosIglesiaAPI.Models.DTOs;

public class DailyVerseDTO
{
    public string Text { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Book { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int Verse { get; set; }
    public DateTime Date { get; set; }
    public string? Theme { get; set; }
}

public class BibleApiResponse
{
    public string Text { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string BookName { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int Verse { get; set; }
}
