using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiCourseService : ICourseService
{
    private readonly HttpClient _httpClient;

    public ApiCourseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>("api/courses") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<List<Course>> GetFeaturedCoursesAsync()
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>("api/courses/featured") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<List<Course>> GetCoursesByCategoryAsync(Guid categoryId)
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/category/{categoryId}") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<List<Course>> SearchCoursesAsync(CourseSearchRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/courses/search", request);
        var courses = await response.Content.ReadFromJsonAsync<List<Course>>() ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        var course = await _httpClient.GetFromJsonAsync<Course>($"api/courses/{id}");
        if (course != null) FixUrls(course);
        return course;
    }

    public async Task<List<Course>> GetPopularCoursesAsync(int count = 6)
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/popular/{count}") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<List<Course>> GetRecentCoursesAsync(int count = 6)
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/recent/{count}") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }
    
    // Helper to map relative URLs to absolute
    private void FixUrls(Course course)
    {
        if (course == null) return;
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl))
        {
            if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl.StartsWith("/"))
                course.ImageUrl = baseUrl + course.ImageUrl;
            if (!string.IsNullOrEmpty(course.InstructorImageUrl) && course.InstructorImageUrl.StartsWith("/"))
                course.InstructorImageUrl = baseUrl + course.InstructorImageUrl;
        }
    }
}
