namespace CursosIglesia.Models;

public class ActivityResponse
{
    public Guid IdActivityResponse { get; set; }
    public Guid IdActividad { get; set; }
    public Guid IdPregunta { get; set; }
    public Guid IdUsuario { get; set; }
    public string Respuesta { get; set; } = string.Empty; // JSON flexible
    public int? PuntosGanados { get; set; }
    public DateTime FechaEnvio { get; set; }
    public string Feedback { get; set; } = string.Empty; // Comentario del maestro
    public DateTime FechaCreacion { get; set; }
}
