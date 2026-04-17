using CursosIglesiaAPI.Models.DTOs;

namespace CursosIglesiaAPI.Services.Interfaces;

public interface ICertificateService
{
    Task<CertificateResponse> GenerateCertificateAsync(Guid userId, Guid courseId);
    Task<List<CertificateResponse>> GetUserCertificatesAsync(Guid userId);
    Task<CertificateResponse?> GetCertificateDetailsAsync(Guid certificateId, Guid userId);
    Task<VerifyCertificateResponse?> VerifyCertificateAsync(string certificateNumber);
    Task<byte[]> GetCertificatePdfAsync(Guid certificateId, Guid userId);
}
