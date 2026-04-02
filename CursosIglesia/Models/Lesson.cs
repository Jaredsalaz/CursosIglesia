namespace CursosIglesia.Models;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Tema> Topics { get; set; } = new();
}
