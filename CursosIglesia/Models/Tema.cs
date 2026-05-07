namespace CursosIglesia.Models;

public class Tema
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsFree { get; set; }
}

public enum ContentType
{
    Lectura = 0,
    VideoSubido = 1,
    VideoExterno = 2,
    ClaseVirtual = 3,
    Archivo = 4,
    Test = 5,
    Activity = 6
}
