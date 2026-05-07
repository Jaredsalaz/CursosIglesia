namespace CursosIglesia.Models;

public class ActivityQuestion
{
    public Guid IdActivityQuestion { get; set; }
    public Guid IdActividad { get; set; }
    public string TextoPregunta { get; set; } = string.Empty;
    public int TipoPregunta { get; set; }
    public int Orden { get; set; }
    public bool EsObligatoria { get; set; } = true;
    public int Puntos { get; set; } = 10;
    public string Opciones { get; set; } = string.Empty; // JSON para múltiple choice
    public string RespuestaCorrecta { get; set; } = string.Empty; // JSON respuesta correcta
    public string TextoFeedback { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public List<ActivityResponse> Responses { get; set; } = new();
}

public enum ActivityQuestionType
{
    ShortText = 0,
    LongText = 1,
    MultipleChoice = 2,
    Ranking = 3,
    Rating = 4,
    Matching = 5,
    FileUpload = 6
}
