namespace CursosIglesia.Models;

public class Forum
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? TemaId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ForumPost> Posts { get; set; } = new();
}
