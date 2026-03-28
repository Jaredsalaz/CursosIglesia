using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiUserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiUserService(HttpClient httpClient, ILocalStorageService localStorage)
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

    public async Task<UserProfile> GetProfileAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/user/profile");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfile>() ?? new();
    }

    public async Task<ProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "api/user/profile")
        {
            Content = JsonContent.Create(request)
        };
        await AddAuthorizationHeaderAsync(httpRequest);
        using var response = await _httpClient.SendAsync(httpRequest);
        return await response.Content.ReadFromJsonAsync<ProfileResponse>() ?? new();
    }

    public async Task<ProfileResponse> UpdatePasswordAsync(ChangePasswordRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "api/user/password")
        {
            Content = JsonContent.Create(request)
        };
        await AddAuthorizationHeaderAsync(httpRequest);
        using var response = await _httpClient.SendAsync(httpRequest);
        return await response.Content.ReadFromJsonAsync<ProfileResponse>() ?? new();
    }

    public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/user/payment-methods");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PaymentMethod>>() ?? new();
    }

    public async Task AddPaymentMethodAsync(PaymentMethod method)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/user/payment-methods")
        {
            Content = JsonContent.Create(method)
        };
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemovePaymentMethodAsync(Guid methodId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/user/payment-methods/{methodId}");
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateNotificationsAsync(NotificationPreferences preferences)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "api/user/notifications")
        {
            Content = JsonContent.Create(preferences)
        };
        await AddAuthorizationHeaderAsync(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
