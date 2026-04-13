using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> ActivateUserAsync(string email);
    Task<AuthResponse> ResetPasswordWithEmailAsync(string email, string newPasswordHash);
    Task<bool> UserExistsAsync(string email);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    UserProfile? CurrentUser { get; }
    event Action? OnAuthStateChanged;
}
