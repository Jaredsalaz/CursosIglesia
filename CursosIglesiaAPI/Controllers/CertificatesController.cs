using System.Security.Claims;
using CursosIglesiaAPI.Models.DTOs;
using CursosIglesiaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        [HttpPost("generate/{courseId}")]
        [Authorize]
        public async Task<ActionResult<CertificateResponse>> GenerateCertificate(Guid courseId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized(new { message = "Token inválido o expirado." });
            }

            // URL del frontend que sirve para verificación: http://localhost:5114/verify/{number}
            var baseUrl = $"{Request.Scheme}://localhost:5114"; // Frontend URL - Se puede parametrizar luego
            var verificationUrlPattern = $"{baseUrl}/verify/{{number}}";

            var response = await _certificateService.GenerateCertificateAsync(userId, courseId, verificationUrlPattern);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CertificateListDto>>> GetMyCertificates()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var result = await _certificateService.GetMyCertificatesAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CertificateListDto>> GetCertificate(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var cert = await _certificateService.GetCertificateByIdAsync(id);
            if (cert == null) return NotFound();

            if (cert.IdUsuario != userId)
            {
                // Simple comprobación de seguridad: un usuario normal sólo puede ver sus propios certificados.
                return Forbid();
            }

            return Ok(cert);
        }

        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var cert = await _certificateService.GetCertificateByIdAsync(id);
            if (cert == null) return NotFound("Certificado no encontrado.");
            if (cert.IdUsuario != userId) return Forbid();

            try
            {
                var pdfBytes = Services.Implementations.CertificatePdfGenerator.GeneratePdf(cert);
                return File(pdfBytes, "application/pdf", $"Certificado_{cert.NumeroCertificado}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF del certificado {Id}", id);
                return StatusCode(500, "Error interno generando el documento PDF.");
            }
        }

        [HttpGet("verify/{number}")]
        // Endpoint Público sin [Authorize]
        public async Task<ActionResult<VerifyCertificateResponse>> VerifyCertificate(string number)
        {
            var response = await _certificateService.VerifyCertificateAsync(number);
            return Ok(response);
        }
    }
}
