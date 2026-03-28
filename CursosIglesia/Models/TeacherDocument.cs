namespace CursosIglesia.Models;

public class TeacherDocument
{
    public Guid Id { get; set; }
    public Guid IdMaestro { get; set; }
    public Guid? IdCurso { get; set; }
    public string NombreCursoAsociado { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string UrlArchivo { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }
}
