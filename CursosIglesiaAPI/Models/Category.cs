namespace CursosIglesia.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int CourseCount { get; set; }
}
