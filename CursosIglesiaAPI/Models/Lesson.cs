namespace CursosIglesia.Models;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsFree { get; set; }
    public LessonType Type { get; set; }
}

public enum LessonType
{
    Video,
    Lectura,
    Cuestionario,
    Actividad
}
