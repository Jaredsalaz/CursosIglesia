using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface ICourseService
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<List<Course>> GetFeaturedCoursesAsync();
    Task<List<Course>> GetCoursesByCategoryAsync(Guid categoryId);
    Task<List<Course>> SearchCoursesAsync(CourseSearchRequest request);
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<List<Course>> GetPopularCoursesAsync(int count = 6);
    Task<List<Course>> GetRecentCoursesAsync(int count = 6);
}
