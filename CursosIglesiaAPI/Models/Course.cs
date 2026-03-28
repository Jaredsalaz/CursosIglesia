namespace CursosIglesia.Models;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorImageUrl { get; set; } = string.Empty;
    public string InstructorBio { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public int LessonsCount { get; set; }
    public int StudentsEnrolled { get; set; }
    public double Rating { get; set; }
    public int RatingsCount { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsFree { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<string> Topics { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
}

public enum DifficultyLevel
{
    Principiante,
    Intermedio,
    Avanzado
}
