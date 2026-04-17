using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.ViewModels;

public class CertificatesViewModel
{
    private readonly ICertificateService _certificateService;
    private readonly ILogger<CertificatesViewModel> _logger;

    public List<CertificateResponse> MyCertificates { get; set; } = new();
    public bool IsLoading { get; set; } = false;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public CertificatesViewModel(ICertificateService certificateService, ILogger<CertificatesViewModel> logger)
    {
        _certificateService = certificateService;
        _logger = logger;
    }

    /// <summary>
    /// Carga los certificados del usuario
    /// </summary>
    public async Task LoadMyCertificatesAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            MyCertificates = await _certificateService.GetMyDiplomasAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al cargar certificados: {ex.Message}");
            ErrorMessage = "No se pudieron cargar los certificados.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Genera un nuevo certificado para un curso
    /// </summary>
    public async Task GenerateCertificateAsync(Guid courseId)
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var certificate = await _certificateService.GenerateCertificateAsync(courseId);
            MyCertificates.Add(certificate);
            SuccessMessage = "¡Certificado generado exitosamente!";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al generar certificado: {ex.Message}");
            ErrorMessage = "No se pudo generar el certificado. Por favor intenta más tarde.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Descarga el PDF del certificado
    /// </summary>
    public async Task DownloadCertificateAsync(CertificateResponse certificate)
    {
        try
        {
            var pdfBytes = await _certificateService.DownloadCertificatePdfAsync(certificate.IdCertificado);

            if (pdfBytes.Length == 0)
            {
                ErrorMessage = "Error al descargar el certificado.";
                return;
            }

            // Disparar descarga en el cliente
            await DownloadFileAsync(pdfBytes, $"Certificado-{certificate.NumeroCertificado}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar certificado: {ex.Message}");
            ErrorMessage = "No se pudo descargar el certificado.";
        }
    }

    /// <summary>
    /// Copia el link de verificación al portapapeles
    /// </summary>
    public string GetVerificationLink(string certificateNumber)
    {
        return $"https://iglesia.com/verify/{certificateNumber}";
    }

    // Helper para descargar archivo (será llamado desde Razor con JS interop)
    public async Task DownloadFileAsync(byte[] content, string fileName)
    {
        // Este método será extendido en Razor con JavaScript interop
        // Por ahora solo preparamos los bytes
        await Task.CompletedTask;
    }
}
