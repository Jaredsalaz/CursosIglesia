using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiCertificateService : ICertificateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiCertificateService> _logger;

    public ApiCertificateService(HttpClient httpClient, ILogger<ApiCertificateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Genera un certificado para el curso completado
    /// </summary>
    public async Task<CertificateResponse> GenerateCertificateAsync(Guid courseId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/certificates/generate/{courseId}", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Error al generar certificado: {errorContent}");
                throw new InvalidOperationException("No se pudo generar el certificado");
            }

            var certificate = await response.Content.ReadFromJsonAsync<CertificateResponse>();
            return certificate ?? throw new InvalidOperationException("Respuesta vacía del servidor");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al generar certificado: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los certificados del usuario
    /// </summary>
    public async Task<List<CertificateResponse>> GetMyDiplomasAsync()
    {
        try
        {
            var certificates = await _httpClient.GetFromJsonAsync<List<CertificateResponse>>("api/certificates") ?? new();
            return certificates;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener certificados: {ex.Message}");
            return new List<CertificateResponse>();
        }
    }

    /// <summary>
    /// Obtiene un certificado específico
    /// </summary>
    public async Task<CertificateResponse?> GetCertificateAsync(Guid certificateId)
    {
        try
        {
            var certificate = await _httpClient.GetFromJsonAsync<CertificateResponse>($"api/certificates/{certificateId}");
            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener certificado: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Descarga el PDF del certificado
    /// </summary>
    public async Task<byte[]> DownloadCertificatePdfAsync(Guid certificateId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/certificates/{certificateId}/download");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Error al descargar certificado: {response.StatusCode}");
                return Array.Empty<byte>();
            }

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar PDF: {ex.Message}");
            return Array.Empty<byte>();
        }
    }
}
