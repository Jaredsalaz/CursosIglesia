namespace CursosIglesia.Models.DTOs
{
    public class CertificateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? IdCertificado { get; set; }
        public string? NumeroCertificado { get; set; }
        public DateTime? FechaOtorgamiento { get; set; }
    }

    public class CertificateListDto
    {
        public Guid IdCertificado { get; set; }
        public string NumeroCertificado { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string NombreInstructor { get; set; } = string.Empty;
        public DateTime FechaOtorgamiento { get; set; }
        public string? CodigoQR { get; set; }
        public Guid? IdUsuario { get; set; }
    }

    public class VerifyCertificateResponse
    {
        public bool Valido { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public DateTime? FechaOtorgamiento { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
