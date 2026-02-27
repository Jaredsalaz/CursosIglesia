using CursosIglesia.Models;

namespace CursosIglesia.Services.Interfaces;

public interface ICourseService
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<List<Course>> GetFeaturedCoursesAsync();
    Task<List<Course>> GetCoursesByCategoryAsync(int categoryId);
    Task<List<Course>> SearchCoursesAsync(string query);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<List<Course>> GetPopularCoursesAsync(int count = 6);
    Task<List<Course>> GetRecentCoursesAsync(int count = 6);
}
