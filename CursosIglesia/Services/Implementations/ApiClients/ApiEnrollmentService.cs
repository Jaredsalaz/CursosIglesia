using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiEnrollmentService : IEnrollmentService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiEnrollmentService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<bool> EnrollAsync(Guid courseId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/enrollment/enroll/{courseId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnenrollAsync(Guid courseId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/enrollment/unenroll/{courseId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsEnrolledAsync(Guid courseId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/enrollment/check/{courseId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<List<Enrollment>> GetEnrollmentsAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/enrollment");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Enrollment>>() ?? new();
    }

    public async Task<Enrollment?> GetEnrollmentAsync(Guid courseId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/enrollment/{courseId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Enrollment>();
    }

    public async Task<bool> CompleteLessonAsync(LessonUpdateProgressRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/enrollment/complete-lesson")
        {
            Content = JsonContent.Create(request)
        };
        await AddAuthorizationHeaderAsync(httpRequest);
        using var response = await _httpClient.SendAsync(httpRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetCurrentLessonAsync(Guid courseId, Guid lessonId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/enrollment/current-lesson/{courseId}/{lessonId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
