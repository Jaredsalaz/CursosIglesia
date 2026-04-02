using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Parish { get; set; }
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfile? Profile { get; set; }
    public string? Token { get; set; }
    public string? Role { get; set; }
}
