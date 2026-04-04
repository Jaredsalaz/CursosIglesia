using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

public class CourseSearchRequest
{
    public string? Query { get; set; }
    public Guid? CategoryId { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public string SortBy { get; set; } = "popular";
}

public class CourseResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class LessonUpdateProgressRequest
{
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public int TotalLessons { get; set; }
}

public class TopicUpdateProgressRequest
{
    public Guid CourseId { get; set; }
    public Guid TopicId { get; set; }
    public int TotalTopics { get; set; }
}
