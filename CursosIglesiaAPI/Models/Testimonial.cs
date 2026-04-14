namespace CursosIglesia.Models;

public class Testimonial
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentImageUrl { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsApproved { get; set; }
    public DateTime Date { get; set; }
}
