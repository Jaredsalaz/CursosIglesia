using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Parish { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfile? Profile { get; set; }
}
