namespace CursosIglesia.Models;

public class Enrollment
{
    public int CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public double Progress { get; set; }
    public List<int> CompletedLessonIds { get; set; } = new();
    public int? CurrentLessonId { get; set; }
    public bool IsCompleted { get; set; }
}
