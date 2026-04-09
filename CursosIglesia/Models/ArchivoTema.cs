namespace CursosIglesia.Models;

public class ArchivoTema
{
    public Guid Id { get; set; }
    public Guid TemaId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreServidor { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; }
}
