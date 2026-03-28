using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    UserProfile? CurrentUser { get; }
    event Action? OnAuthStateChanged;
    Task InitializeAsync();
}
