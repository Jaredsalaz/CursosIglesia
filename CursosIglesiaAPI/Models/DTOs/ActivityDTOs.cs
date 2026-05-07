using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

// ===== Create Activity Request =====
public class CreateActivityRequest
{
    public Guid IdTema { get; set; }
    public ActivityType Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Instrucciones { get; set; } = string.Empty;
    public DateTime? FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool MostrarRespuestas { get; set; } = false;
    public bool PermitirEnvioTarde { get; set; } = true;
    public List<CreateActivityQuestionRequest> Preguntas { get; set; } = new();
}

// ===== Create Activity Question Request =====
public class CreateActivityQuestionRequest
{
    public string TextoPregunta { get; set; } = string.Empty;
    public ActivityQuestionType TipoPregunta { get; set; }
    public int Orden { get; set; }
    public bool EsObligatoria { get; set; } = true;
    public int Puntos { get; set; } = 10;
    public string[]? Opciones { get; set; } // Para múltiple choice
    public string? RespuestaCorrecta { get; set; } // JSON
    public string? TextoFeedback { get; set; }
}

// ===== Activity Response DTO =====
public class ActivityResponseDto
{
    public Guid Id { get; set; }
    public Guid IdActividad { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdPregunta { get; set; }
    public string TextoPregunta { get; set; } = string.Empty;
    public string Respuesta { get; set; } = string.Empty;
    public int? PuntosGanados { get; set; }
    public DateTime FechaEnvio { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

// ===== Submit Activity Response Request =====
public class SubmitActivityResponseRequest
{
    public Guid IdActividad { get; set; }
    public Dictionary<Guid, string> Respuestas { get; set; } = new();
}

// ===== Activity Statistics =====
public class ActivityStatsDto
{
    public Guid IdActividad { get; set; }
    public string TituloActividad { get; set; } = string.Empty;
    public int TotalEstudiantes { get; set; }
    public int RespuestasCompletadas { get; set; }
    public double PorcentajeCompletado { get; set; }
    public double PuntajePromedio { get; set; }
    public List<QuestionStatsDto> EstadisticasPreguntas { get; set; } = new();
}

// ===== Question Statistics =====
public class QuestionStatsDto
{
    public Guid IdPregunta { get; set; }
    public string TextoPregunta { get; set; } = string.Empty;
    public ActivityQuestionType TipoPregunta { get; set; }
    public int TotalRespuestas { get; set; }
    public double PromedioPuntos { get; set; }
    public Dictionary<string, int> FrecuenciaRespuestas { get; set; } = new(); // Para múltiple choice
}

// ===== Add Feedback Request =====
public class AddFeedbackRequest
{
    public Guid IdRespuesta { get; set; }
    public int PuntosGanados { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

// ===== Activity Response List Item =====
public class StudentActivityResponseItem
{
    public Guid IdUsuario { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public string EmailEstudiante { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
    public int? TotalPuntosGanados { get; set; }
    public int PuntosMaximos { get; set; }
    public bool EstaCompletado { get; set; }
}
