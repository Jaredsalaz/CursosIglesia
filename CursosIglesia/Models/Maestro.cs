namespace CursosIglesia.Models;

public class Maestro
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    public int ExperienciaAnios { get; set; }
    public bool Activo { get; set; }
}
