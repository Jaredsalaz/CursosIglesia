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
        return await _httpClient.GetFromJsonAsync<List<Course>>("api/courses") ?? new();
    }

    public async Task<List<Course>> GetFeaturedCoursesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>("api/courses/featured") ?? new();
    }

    public async Task<List<Course>> GetCoursesByCategoryAsync(Guid categoryId)
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/category/{categoryId}") ?? new();
    }

    public async Task<List<Course>> SearchCoursesAsync(CourseSearchRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/courses/search", request);
        return await response.Content.ReadFromJsonAsync<List<Course>>() ?? new();
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Course>($"api/courses/{id}");
    }

    public async Task<List<Course>> GetPopularCoursesAsync(int count = 6)
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/popular/{count}") ?? new();
    }

    public async Task<List<Course>> GetRecentCoursesAsync(int count = 6)
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>($"api/courses/recent/{count}") ?? new();
    }
}
