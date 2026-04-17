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
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, IOtpService otpService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
        _otpService = otpService;
        _emailService = emailService;
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

        // Enviar OTP
        var otp = _otpService.GenerateOtp(request.Email);
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                <h2 style='color: #2e3192;'>Bienvenido a CursosIglesia</h2>
                <p>Hola {request.FirstName},</p>
                <p>Tu código de seguridad para activar tu cuenta es:</p>
                <h1 style='color: #4CAF50; font-size: 32px; letter-spacing: 5px; text-align: center; border: 1px solid #4CAF50; padding: 10px; border-radius: 8px;'>{otp}</h1>
                <p style='color: #555;'>Este código expirará en 10 minutos. Por favor no compartas este código con nadie.</p>
            </div>";

        await _emailService.SendEmailAsync(request.Email, "Código de Activación - CursosIglesia", emailBody);

        return Ok(response);
    }

    [HttpPost("verify-registration-otp")]
    public async Task<ActionResult<AuthResponse>> VerifyRegistrationOtp([FromBody] VerifyOtpRequest request)
    {
        _logger.LogInformation($"VerifyRegistrationOtp attempt for email: '{request?.Email}', otp code: '{request?.Otp}'");
        
        bool isValid = _otpService.VerifyOtp(request.Email?.Trim() ?? "", request.Otp?.Trim() ?? "");
        if (!isValid) 
        {
            _logger.LogWarning($"OTP Validation failed for email: {request.Email}");
            return BadRequest(new AuthResponse { Success = false, Message = "Código OTP inválido o expirado." });
        }

        var response = await _authService.ActivateUserAsync(request.Email?.Trim() ?? "");
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<AuthResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        bool exists = await _authService.UserExistsAsync(request.Email);
        // Retornamos OK incluso si no existe (prevención de escaneo de correos)
        if (!exists) return Ok(new AuthResponse { Success = true, Message = "Si el correo está registrado, recibirás un OTP con instrucciones." });

        var otp = _otpService.GenerateOtp(request.Email);
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                <h2 style='color: #e53935;'>Recuperación de Contraseña - CursosIglesia</h2>
                <p>Hola,</p>
                <p>Recientemente solicitaste restablecer la contraseña para tu cuenta de CursosIglesia.</p>
                <p>Tu código OTP de un solo uso es:</p>
                <h1 style='color: #FF9800; font-size: 32px; letter-spacing: 5px; text-align: center; border: 1px dashed #FF9800; padding: 10px;'>{otp}</h1>
                <p style='color: #555;'>Si no solicitaste este cambio, simplemente puedes ignorar este correo.</p>
            </div>";

        await _emailService.SendEmailAsync(request.Email, "Recuperación de Contraseña", emailBody);
        
        return Ok(new AuthResponse { Success = true, Message = "Si el correo está registrado, recibirás un OTP con instrucciones." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        bool isValid = _otpService.VerifyOtp(request.Email, request.Otp);
        if (!isValid) return BadRequest(new AuthResponse { Success = false, Message = "Código OTP inválido o expirado." });

        string newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var response = await _authService.ResetPasswordWithEmailAsync(request.Email, newHash);
        return response.Success ? Ok(response) : BadRequest(response);
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
