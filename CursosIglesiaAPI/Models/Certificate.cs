using CursosIglesiaAPI.Models;

namespace CursosIglesiaAPI.Models
{
    public class Certificate
    {
        public Guid IdCertificado { get; set; }
        public Guid IdUsuario { get; set; }
        public Guid IdCurso { get; set; }
        public string NumeroCertificado { get; set; } = string.Empty;
        public DateTime FechaOtorgamiento { get; set; }
        public string? CodigoQR { get; set; }
        public string? PreviewImageUrl { get; set; }
    }
}
