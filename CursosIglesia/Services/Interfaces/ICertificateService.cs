using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface ICertificateService
{
    Task<CertificateResponse> GenerateCertificateAsync(Guid courseId);
    Task<List<CertificateResponse>> GetMyDiplomasAsync();
    Task<CertificateResponse?> GetCertificateAsync(Guid certificateId);
    Task<byte[]> DownloadCertificatePdfAsync(Guid certificateId);
}
