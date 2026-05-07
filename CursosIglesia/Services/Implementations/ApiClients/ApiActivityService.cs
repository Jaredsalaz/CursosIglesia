using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiActivityService : IActivityService
{
    private readonly HttpClient _httpClient;

    public ApiActivityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Guid>> CreateActivityAsync(Guid userId, CreateActivityRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/activities", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() ?? new();
    }

    public async Task<Activity?> GetActivityAsync(Guid activityId)
    {
        return await _httpClient.GetFromJsonAsync<Activity>($"api/activities/{activityId}");
    }

    public async Task<List<Activity>> GetActivitiesByTemaAsync(Guid temaId)
    {
        return await _httpClient.GetFromJsonAsync<List<Activity>>($"api/topics/{temaId}/activities") ?? new();
    }

    public async Task<ApiResponse> UpdateActivityAsync(Guid activityId, CreateActivityRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/activities/{activityId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> DeleteActivityAsync(Guid activityId)
    {
        var response = await _httpClient.DeleteAsync($"api/activities/{activityId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> SubmitResponseAsync(Guid userId, SubmitActivityResponseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/activities/submit-response", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<List<ActivityResponseDto>> GetActivityResponsesAsync(Guid activityId)
    {
        return await _httpClient.GetFromJsonAsync<List<ActivityResponseDto>>($"api/activities/{activityId}/responses") ?? new();
    }

    public async Task<List<ActivityResponseDto>> GetStudentResponseAsync(Guid userId, Guid activityId)
    {
        return await _httpClient.GetFromJsonAsync<List<ActivityResponseDto>>($"api/activities/{activityId}/student/{userId}") ?? new();
    }

    public async Task<List<StudentActivityResponseItem>> GetStudentSubmissionsAsync(Guid activityId)
    {
        return await _httpClient.GetFromJsonAsync<List<StudentActivityResponseItem>>($"api/activities/{activityId}/submissions") ?? new();
    }

    public async Task<ApiResponse> AddFeedbackAsync(AddFeedbackRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/activities/feedback", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ActivityStatsDto?> GetActivityStatsAsync(Guid activityId)
    {
        return await _httpClient.GetFromJsonAsync<ActivityStatsDto>($"api/activities/{activityId}/stats");
    }

    public async Task<bool> HasUngradedActivitiesAsync(Guid courseId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/activities/course/{courseId}/ungraded");
    }
}
