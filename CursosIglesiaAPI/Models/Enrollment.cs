namespace CursosIglesia.Models;

public class Enrollment
{
    public Guid CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public double Progress { get; set; }
    public List<Guid> CompletedLessonIds { get; set; } = new();
    public Guid? CurrentLessonId { get; set; }
    public bool IsCompleted { get; set; }
}
