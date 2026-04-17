using CursosIglesiaAPI.Models.DTOs;

namespace CursosIglesiaAPI.Services.Interfaces
{
    public interface ICertificateService
    {
        Task<CertificateResponse> GenerateCertificateAsync(Guid userId, Guid courseId, string verificationUrlPattern);
        Task<IEnumerable<CertificateListDto>> GetMyCertificatesAsync(Guid userId);
        Task<CertificateListDto?> GetCertificateByIdAsync(Guid certificateId);
        Task<VerifyCertificateResponse> VerifyCertificateAsync(string numeroCertificado);
    }
}
