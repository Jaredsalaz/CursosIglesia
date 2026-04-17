using CursosIglesiaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CursosIglesiaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;
    private readonly ILogger<CertificatesController> _logger;

    public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
    {
        _certificateService = certificateService;
        _logger = logger;
    }

    /// <summary>
    /// Genera un certificado para el usuario autenticado
    /// </summary>
    [HttpPost("generate/{courseId}")]
    [Authorize]
    public async Task<IActionResult> GenerateCertificate(Guid courseId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized("Usuario no identificado");

            var certificate = await _certificateService.GenerateCertificateAsync(userIdGuid, courseId);
            return Ok(certificate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Error al generar certificado: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado al generar certificado: {ex}");
            return StatusCode(500, new { error = "Error al generar certificado" });
        }
    }

    /// <summary>
    /// Obtiene todos los certificados del usuario autenticado
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyCertificates()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized("Usuario no identificado");

            var certificates = await _certificateService.GetUserCertificatesAsync(userIdGuid);
            return Ok(certificates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener certificados: {ex}");
            return StatusCode(500, new { error = "Error al obtener certificados" });
        }
    }

    /// <summary>
    /// Obtiene detalles de un certificado específico
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized("Usuario no identificado");

            var certificate = await _certificateService.GetCertificateDetailsAsync(id, userIdGuid);
            if (certificate == null)
                return NotFound("Certificado no encontrado");

            return Ok(certificate);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener detalle de certificado: {ex}");
            return StatusCode(500, new { error = "Error al obtener certificado" });
        }
    }

    /// <summary>
    /// Descarga el PDF del certificado
    /// </summary>
    [HttpGet("{id}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadCertificate(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized("Usuario no identificado");

            var pdfBytes = await _certificateService.GetCertificatePdfAsync(id, userIdGuid);

            return File(
                pdfBytes,
                "application/pdf",
                $"Certificado-{DateTime.Now:yyyy-MM-dd}.pdf"
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Error al descargar certificado: {ex.Message}");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar certificado PDF: {ex}");
            return StatusCode(500, new { error = "Error al descargar certificado" });
        }
    }

    /// <summary>
    /// Verifica un certificado (endpoint público)
    /// </summary>
    [HttpGet("verify/{certificateNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCertificate(string certificateNumber)
    {
        try
        {
            var result = await _certificateService.VerifyCertificateAsync(certificateNumber);
            if (result == null)
                return NotFound(new { valido = false, mensaje = "Certificado no encontrado" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al verificar certificado: {ex}");
            return StatusCode(500, new { error = "Error al verificar certificado" });
        }
    }
}
