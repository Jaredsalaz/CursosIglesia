using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiForumService : IForumService
{
    private readonly HttpClient _httpClient;

    public ApiForumService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Guid>> CreateForumAsync(Guid userId, CreateForumRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/forums", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() ?? new();
    }

    public async Task<ForumDto?> GetForumAsync(Guid forumId)
    {
        return await _httpClient.GetFromJsonAsync<ForumDto>($"api/forums/{forumId}");
    }

    public async Task<List<ForumDto>> GetCourseForumsAsync(Guid courseId)
    {
        return await _httpClient.GetFromJsonAsync<List<ForumDto>>($"api/forums/course/{courseId}") ?? new();
    }

    public async Task<ApiResponse> DeleteForumAsync(Guid forumId)
    {
        var response = await _httpClient.DeleteAsync($"api/forums/{forumId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse<Guid>> CreatePostAsync(Guid userId, CreateForumPostRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/forums/posts", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() ?? new();
    }

    public async Task<List<ForumPostDto>> GetForumPostsAsync(Guid forumId, Guid? currentUserId = null)
    {
        var url = currentUserId.HasValue
            ? $"api/forums/{forumId}/posts?currentUserId={currentUserId}"
            : $"api/forums/{forumId}/posts";
        return await _httpClient.GetFromJsonAsync<List<ForumPostDto>>(url) ?? new();
    }

    public async Task<ForumPostDto?> GetPostAsync(Guid postId, Guid? currentUserId = null)
    {
        var url = currentUserId.HasValue
            ? $"api/forums/posts/{postId}?currentUserId={currentUserId}"
            : $"api/forums/posts/{postId}";
        return await _httpClient.GetFromJsonAsync<ForumPostDto>(url);
    }

    public async Task<ApiResponse> UpdatePostAsync(Guid postId, UpdateForumPostRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/forums/posts/{postId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> DeletePostAsync(Guid postId)
    {
        var response = await _httpClient.DeleteAsync($"api/forums/posts/{postId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> PinPostAsync(Guid postId, bool isPinned)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/forums/posts/{postId}/pin", new { isPinned });
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> LikePostAsync(Guid userId, Guid postId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/forums/posts/{postId}/like", new { userId });
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ApiResponse> UnlikePostAsync(Guid userId, Guid postId)
    {
        var response = await _httpClient.DeleteAsync($"api/forums/posts/{postId}/like/{userId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new();
    }

    public async Task<ForumEngagementStatsDto?> GetForumStatsAsync(Guid forumId)
    {
        return await _httpClient.GetFromJsonAsync<ForumEngagementStatsDto>($"api/forums/{forumId}/stats");
    }
}
