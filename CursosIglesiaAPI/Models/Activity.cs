namespace CursosIglesia.Models;

public class Activity
{
    public Guid IdActividad { get; set; }
    public Guid IdTema { get; set; }
    public int Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Instrucciones { get; set; } = string.Empty;
    public DateTime? FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool MostrarRespuestas { get; set; } = false;
    public bool PermitirEnvioTarde { get; set; } = true;
    public Guid IdUsuarioCreador { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<ActivityQuestion> Questions { get; set; } = new();
    public List<ActivityResponse> Responses { get; set; } = new();
}

public enum ActivityType
{
    Discussion = 0,
    Assignment = 1,
    Poll = 2,
    Forum = 3
}
