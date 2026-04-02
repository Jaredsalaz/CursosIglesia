using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiAuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly InMemoryTokenStore _tokenStore;
    public UserProfile? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action? OnAuthStateChanged;
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _tokenStore.Token = token;
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(token);
            }

            var profile = await _localStorage.GetItemAsync<UserProfile>("userProfile");
            if (profile != null)
            {
                CurrentUser = profile;
                NotifyStateChange();
            }
        }
        catch
        {
            // Ignore errors during hydration (e.g. pre-rendering)
        }
        finally
        {
            _isInitialized = true;
        }
    }

    public ApiAuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, InMemoryTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _tokenStore = tokenStore;
    }

    private void NotifyStateChange() => OnAuthStateChanged?.Invoke();

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
            {
                // Store in memory for HttpClient handlers (no JS interop needed)
                _tokenStore.Token = result.Token;
                
                // Store in localStorage for persistence across page reloads
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("userProfile", result.Profile);
                
                await Task.Delay(100);
                
                // Update auth state
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                CurrentUser = result.Profile;
                NotifyStateChange();
                
                // Verify token is in localStorage
                var verifyToken = await _localStorage.GetItemAsync<string>("authToken");
                Console.WriteLine($"[ApiAuthService] Verification: Token in storage = {(!string.IsNullOrEmpty(verifyToken))}");
            }
            
            return result ?? new AuthResponse { Success = false, Message = $"Error en login - Status: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiAuthService] ❌ Login error: {ex.Message}");
            return new AuthResponse { Success = false, Message = $"Error de conexión: {ex.Message}" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
        {
            _tokenStore.Token = result.Token;
            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("userProfile", result.Profile);
            ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
            CurrentUser = result.Profile;
            NotifyStateChange();
        }

        return result ?? new AuthResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task LogoutAsync()
    {
        _tokenStore.Clear();
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userProfile");
        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
        CurrentUser = null;
        NotifyStateChange();
    }
}
