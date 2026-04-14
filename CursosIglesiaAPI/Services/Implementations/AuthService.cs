using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class AuthService : IAuthService
{
    private sealed class LoginDbUser
    {
        public Guid IdUsuario { get; set; }
        public Guid IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? UrlAvatar { get; set; }
        public string? Biografia { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Parroquia { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }

    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public UserProfile? CurrentUser 
    { 
        get 
        {
            if (!IsAuthenticated)
            {
                Console.WriteLine("[AuthService] Not authenticated");
                return null;
            }
            var user = _httpContextAccessor.HttpContext?.User;
            Console.WriteLine($"[AuthService] User claims count: {user?.Claims.Count() ?? 0}");
            foreach (var claim in user?.Claims ?? new List<Claim>())
            {
                Console.WriteLine($"[AuthService]   Claim: {claim.Type} = {claim.Value}");
            }
            
            var userIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? user?.FindFirst("sub")?.Value;
            Console.WriteLine($"[AuthService] User ID string: {userIdStr}");
            
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var profile = new UserProfile 
                { 
                    Id = userId, 
                    Email = user?.FindFirst(ClaimTypes.Email)?.Value ?? user?.FindFirst("email")?.Value ?? "",
                    FirstName = user?.FindFirst(ClaimTypes.Name)?.Value ?? user?.FindFirst("name")?.Value ?? ""
                };
                Console.WriteLine($"[AuthService] Created profile: {profile.Id}, Email: {profile.Email}");
                return profile;
            }
            Console.WriteLine("[AuthService] Failed to parse user ID as GUID");
            return null;
        }
    }

    public event Action? OnAuthStateChanged;

    public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private string GenerateJwtToken(UserProfile user, string roleName = "Estudiante")
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, roleName)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"] ?? "1440")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Authenticate using SQL Server stored procedure
            using IDbConnection db = new SqlConnection(_connectionString);
            var dbUser = await db.QueryFirstOrDefaultAsync<LoginDbUser>(
                "usp_AutenticacionYUsuarios",
                new { Accion = "Login", Email = request.Email },
                commandType: CommandType.StoredProcedure
            );

            var passwordValid = false;
            if (dbUser != null && !string.IsNullOrWhiteSpace(dbUser.PasswordHash))
            {
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, dbUser.PasswordHash);
                }
                catch
                {
                    passwordValid = false;
                }
            }

            if (dbUser != null && passwordValid)
            {
                // If NombreRol is empty, try to get it from the Roles table
                var roleName = dbUser.NombreRol;
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    roleName = await db.QueryFirstOrDefaultAsync<string>(
                        "SELECT NombreRol FROM Roles WHERE IdRol = @IdRol",
                        new { dbUser.IdRol }
                    ) ?? "Estudiante";
                }

                var user = new UserProfile
                {
                    Id = dbUser.IdUsuario,
                    RoleId = dbUser.IdRol,
                    FirstName = dbUser.Nombre,
                    LastName = dbUser.Apellidos,
                    Email = dbUser.Email,
                    PasswordHash = dbUser.PasswordHash,
                    Phone = dbUser.Telefono ?? string.Empty,
                    AvatarUrl = dbUser.UrlAvatar ?? string.Empty,
                    Bio = dbUser.Biografia ?? string.Empty,
                    BirthDate = dbUser.FechaNacimiento ?? DateTime.MinValue,
                    Parish = dbUser.Parroquia ?? string.Empty,
                    City = dbUser.Ciudad ?? string.Empty,
                    Country = dbUser.Pais ?? string.Empty,
                    JoinedDate = dbUser.FechaRegistro ?? DateTime.UtcNow
                };

                var token = GenerateJwtToken(user, roleName);
                return new AuthResponse { Success = true, Profile = user, Token = token, Role = roleName };
            }

            return new AuthResponse
            {
                Success = false,
                Message = "Correo electrónico o contraseña incorrectos."
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Error al conectar con el servidor: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Use SQL Server for registration
            using IDbConnection db = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Accion", "RegistrarUsuario");
            parameters.Add("@Nombre", request.FirstName);
            parameters.Add("@Apellidos", request.LastName);
            parameters.Add("@Email", request.Email);
            parameters.Add("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(request.Password));
            parameters.Add("@Parroquia", request.Parish ?? "");
            parameters.Add("@Pais", request.Country ?? "México");
            parameters.Add("@UrlAvatar", request.AvatarUrl ?? "");
            parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
            parameters.Add("@IdUsuarioNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await db.ExecuteAsync("usp_AutenticacionYUsuarios", parameters, commandType: CommandType.StoredProcedure);

            bool success = parameters.Get<bool>("@Exito");
            if (success)
            {
                var userId = parameters.Get<Guid>("@IdUsuarioNuevo");
                var newUser = new UserProfile
                {
                    Id = userId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Parish = request.Parish ?? "",
                    Country = request.Country ?? "México",
                    AvatarUrl = request.AvatarUrl ?? "",
                    JoinedDate = DateTime.Now
                };

                // Se retorna success pero sin token
                // var token = GenerateJwtToken(newUser);
                return new AuthResponse { Success = true, Profile = newUser, Message = "Registro exitoso." };
            }

            return new AuthResponse
            {
                Success = false,
                Message = parameters.Get<string>("@MensajeError") ?? "Error al registrar usuario"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Error en registro: {ex.Message}"
            };
        }
    }

    public Task LogoutAsync()
    {
        // Stateless API logout is handled by the client by removing the token
        return Task.CompletedTask;
    }

    public async Task<AuthResponse> ActivateUserAsync(string email)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Accion", "ActivarUsuario");
            parameters.Add("@Email", email);
            parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await db.ExecuteAsync("usp_AutenticacionYUsuarios", parameters, commandType: CommandType.StoredProcedure);

            bool success = parameters.Get<bool>("@Exito");
            return new AuthResponse
            {
                Success = success,
                Message = success ? "Usuario activado con éxito." : (parameters.Get<string>("@MensajeError") ?? "Error al activar usuario")
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<AuthResponse> ResetPasswordWithEmailAsync(string email, string newPasswordHash)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Accion", "CambiarPassword");
            parameters.Add("@Email", email);
            parameters.Add("@PasswordHash", newPasswordHash);
            parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await db.ExecuteAsync("usp_AutenticacionYUsuarios", parameters, commandType: CommandType.StoredProcedure);

            bool success = parameters.Get<bool>("@Exito");
            return new AuthResponse
            {
                Success = success,
                Message = success ? "Contraseña actualizada exitosamente." : (parameters.Get<string>("@MensajeError") ?? "Error al cambiar contraseña")
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var exists = await db.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM Usuarios WHERE Email = @Email",
                new { Email = email });
            return exists == 1;
        }
        catch
        {
            return false;
        }
    }
}
