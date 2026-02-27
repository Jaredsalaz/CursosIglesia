namespace CursosIglesia.Models;

public class Testimonial
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentImageUrl { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime Date { get; set; }
}
