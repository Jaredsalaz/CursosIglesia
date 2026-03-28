using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Login request is null");
            return BadRequest(new AuthResponse { Success = false, Message = "Solicitud inválida - body es nulo" });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Login attempt with empty email");
            return BadRequest(new AuthResponse { Success = false, Message = "Email es requerido" });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning($"Login attempt with empty password for email: {request.Email}");
            return BadRequest(new AuthResponse { Success = false, Message = "Contraseña es requerida" });
        }

        _logger.LogInformation($"Login attempt for email: {request.Email}");
        try
        {
            var response = await _authService.LoginAsync(request);
            
            if (!response.Success)
            {
                _logger.LogWarning($"Login failed for email: {request.Email} - {response.Message}");
                return BadRequest(response);
            }

            _logger.LogInformation($"Login successful for email: {request.Email}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during login for email: {request.Email}");
            return BadRequest(new AuthResponse { Success = false, Message = "Error al procesar login: " + ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("me")]
    public ActionResult<ProfileResponse> GetCurrentUserInfo()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        return Ok(new ProfileResponse 
        { 
            Success = true, 
            Profile = _authService.CurrentUser 
        });
    }
}
