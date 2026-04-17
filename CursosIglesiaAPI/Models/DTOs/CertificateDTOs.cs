namespace CursosIglesiaAPI.Models.DTOs;

public class CertificateResponse
{
    public Guid IdCertificado { get; set; }
    public string NumeroCertificado { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string? NombreInstructor { get; set; }
    public DateTime FechaOtorgamiento { get; set; }
    public string? CodigoQR { get; set; }
    public bool YaGenerado { get; set; }
}

public class VerifyCertificateResponse
{
    public bool Valido { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;
    public DateTime FechaOtorgamiento { get; set; }
    public string Estado { get; set; } = "Válido";
}
