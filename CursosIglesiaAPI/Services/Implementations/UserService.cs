using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class UserService : IUserService
{
    private sealed class LoginValidationUser
    {
        public Guid IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    private sealed class ProfileDbUser
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Parroquia { get; set; }
        public string? Ciudad { get; set; }
        public string? Country { get; set; }
        public DateTime? JoinedDate { get; set; }
    }

    private readonly string _connectionString;
    private readonly IAuthService _authService;

    public UserService(IConfiguration configuration, IAuthService authService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _authService = authService;
    }

    public async Task<UserProfile> GetProfileAsync()
    {
        var currentUser = _authService.CurrentUser;
        Console.WriteLine($"[UserService] GetProfileAsync called. CurrentUser: {(currentUser?.Id ?? Guid.Empty)}");
        Console.WriteLine($"[UserService] IsAuthenticated: {_authService.IsAuthenticated}");
        
        if (currentUser == null)
        {
            Console.WriteLine("[UserService] CurrentUser is null - throwing UnauthorizedAccessException");
            throw new UnauthorizedAccessException();
        }
        
        try
        {
            // Get profile from SQL Server stored procedure
            using IDbConnection db = new SqlConnection(_connectionString);
            Console.WriteLine($"[UserService] Querying database for user: {currentUser.Id}");
            var dbProfile = await db.QueryFirstOrDefaultAsync<ProfileDbUser>(
                "usp_AutenticacionYUsuarios",
                new { Accion = "ObtenerPerfil", IdUsuario = currentUser.Id },
                commandType: CommandType.StoredProcedure
            );
            Console.WriteLine($"[UserService] Database query result: {(dbProfile != null ? "Found" : "Not found")}");
            
            if (dbProfile != null)
            {
                var profile = new UserProfile
                {
                    Id = dbProfile.Id,
                    RoleId = dbProfile.RoleId,
                    FirstName = dbProfile.FirstName,
                    LastName = dbProfile.LastName,
                    Email = dbProfile.Email,
                    Phone = dbProfile.Phone ?? string.Empty,
                    AvatarUrl = dbProfile.AvatarUrl ?? string.Empty,
                    Bio = dbProfile.Bio ?? string.Empty,
                    BirthDate = dbProfile.BirthDate ?? DateTime.MinValue,
                    Parish = dbProfile.Parroquia ?? string.Empty,
                    City = dbProfile.Ciudad ?? string.Empty,
                    Country = dbProfile.Country ?? string.Empty,
                    JoinedDate = dbProfile.JoinedDate ?? DateTime.UtcNow
                };
                return profile;
            }
        }
        catch
        {
            // If database fails, return the user from claims
        }

        // Fallback to the authenticated user info
        return _authService.CurrentUser ?? throw new UnauthorizedAccessException("User claims not found");
    }

    public async Task<ProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
    {
        if (_authService.CurrentUser == null) throw new UnauthorizedAccessException();

        // Use SQL Server
        using IDbConnection db = new SqlConnection(_connectionString);
        await db.ExecuteAsync(
            "usp_AutenticacionYUsuarios",
            new 
            { 
                Accion = "ActualizarPerfil", 
                IdUsuario = _authService.CurrentUser.Id,
                Nombre = request.FirstName,
                Apellidos = request.LastName,
                Email = request.Email,
                Telefono = request.Phone,
                Biografia = request.Bio,
                Parroquia = request.Parish,
                Ciudad = request.City
            },
            commandType: CommandType.StoredProcedure
        );

        var updatedProfile = await GetProfileAsync();
        return new ProfileResponse { Success = true, Message = "Perfil actualizado con éxito", Profile = updatedProfile };
    }

    public async Task<ProfileResponse> UpdatePasswordAsync(ChangePasswordRequest request)
    {
        if (_authService.CurrentUser == null) throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new ProfileResponse { Success = false, Message = "La contraseña actual y la nueva son requeridas." };
        }

        // Validate current password against the stored hash using existing SP login action.
        using IDbConnection db = new SqlConnection(_connectionString);
        var currentUser = _authService.CurrentUser;
        var loginUser = await db.QueryFirstOrDefaultAsync<LoginValidationUser>(
            "usp_AutenticacionYUsuarios",
            new { Accion = "Login", Email = currentUser.Email },
            commandType: CommandType.StoredProcedure
        );

        if (loginUser == null || loginUser.IdUsuario != currentUser.Id)
        {
            return new ProfileResponse { Success = false, Message = "No se pudo validar el usuario actual." };
        }

        var currentPasswordValid = false;
        try
        {
            currentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, loginUser.PasswordHash);
        }
        catch
        {
            currentPasswordValid = false;
        }

        if (!currentPasswordValid)
        {
            return new ProfileResponse { Success = false, Message = "La contraseña actual es incorrecta." };
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Use SQL Server
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CambiarPassword");
        parameters.Add("@IdUsuario", currentUser.Id);
        parameters.Add("@PasswordHash", newPasswordHash);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_AutenticacionYUsuarios", parameters, commandType: CommandType.StoredProcedure);

        var success = parameters.Get<bool>("@Exito");
        var message = parameters.Get<string>("@MensajeError");

        if (!success)
        {
            return new ProfileResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(message) ? "No se pudo actualizar la contraseña." : message
            };
        }

        return new ProfileResponse { Success = true, Message = "Contraseña actualizada con éxito" };
    }

    public Task<List<PaymentMethod>> GetPaymentMethodsAsync() => Task.FromResult(new List<PaymentMethod>());
    public Task AddPaymentMethodAsync(PaymentMethod method) => Task.CompletedTask;
    public Task RemovePaymentMethodAsync(Guid methodId) => Task.CompletedTask;
    public Task UpdateNotificationsAsync(NotificationPreferences preferences) => Task.CompletedTask;
}
