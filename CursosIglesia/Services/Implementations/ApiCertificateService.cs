using System.Net.Http.Json;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations
{
    public class ApiCertificateService : ICertificateService
    {
        private readonly HttpClient _httpClient;

        public ApiCertificateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CertificateResponse> GenerateCertificateAsync(Guid courseId)
        {
            var response = await _httpClient.PostAsync($"api/certificates/generate/{courseId}", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CertificateResponse>();
                return result ?? new CertificateResponse { Success = false, Message = "Error parseando respuesta." };
            }
            
            try 
            {
                var dict = await response.Content.ReadFromJsonAsync<CertificateResponse>();
                return dict ?? new CertificateResponse { Success = false, Message = "API Error." };
            }
            catch
            {
                return new CertificateResponse { Success = false, Message = $"HTTP Error {response.StatusCode}" };
            }
        }

        public async Task<List<CertificateListDto>> GetMyCertificatesAsync()
        {
            var response = await _httpClient.GetAsync("api/certificates");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<CertificateListDto>>();
                return result ?? new List<CertificateListDto>();
            }
            return new List<CertificateListDto>();
        }

        public async Task<byte[]?> DownloadCertificatePdfAsync(Guid certificateId)
        {
            var response = await _httpClient.GetAsync($"api/certificates/{certificateId}/download");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }

        public async Task<VerifyCertificateResponse> VerifyCertificateAsync(string numeroCertificado)
        {
            var response = await _httpClient.GetAsync($"api/certificates/verify/{numeroCertificado}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<VerifyCertificateResponse>();
                return result ?? new VerifyCertificateResponse { Valido = false };
            }
            return new VerifyCertificateResponse { Valido = false, Message = "No se pudo conectar al servidor." };
        }
    }
}
