using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces
{
    public interface ICertificateService
    {
        Task<CertificateResponse> GenerateCertificateAsync(Guid courseId);
        Task<List<CertificateListDto>> GetMyCertificatesAsync();
        Task<byte[]?> DownloadCertificatePdfAsync(Guid certificateId);
        Task<VerifyCertificateResponse> VerifyCertificateAsync(string numeroCertificado);
    }
}
