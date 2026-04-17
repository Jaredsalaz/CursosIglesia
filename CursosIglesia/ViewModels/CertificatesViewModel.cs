using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;
using Microsoft.JSInterop;

namespace CursosIglesia.ViewModels
{
    public class CertificatesViewModel : ViewModelBase
    {
        private readonly ICertificateService _certificateService;
        private readonly IJSRuntime _jsRuntime;

        private List<CertificateListDto> _certificates = new();
        public List<CertificateListDto> Certificates
        {
            get => _certificates;
            set => SetProperty(ref _certificates, value);
        }

        public CertificatesViewModel(ICertificateService certificateService, IJSRuntime jsRuntime)
        {
            _certificateService = certificateService;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadCertificatesAsync()
        {
            IsLoading = true;
            try
            {
                Certificates = await _certificateService.GetMyCertificatesAsync();
                
                // Initialize JS module for downloads if not already logic
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error cargando los certificados: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DownloadCertificateAsync(Guid certificateId, string numero)
        {
            IsLoading = true;
            try
            {
                var pdfBytes = await _certificateService.DownloadCertificatePdfAsync(certificateId);
                if (pdfBytes != null)
                {
                    // Call JS to download the file
                    using var streamRef = new DotNetStreamReference(new MemoryStream(pdfBytes));
                    await _jsRuntime.InvokeVoidAsync("certificates.downloadFileFromStream", $"Certificado_{numero}.pdf", streamRef);
                }
                else
                {
                    ErrorMessage = "No se pudo descargar el certificado.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error en la descarga: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
