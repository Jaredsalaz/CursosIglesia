using System;
using System.Collections.Generic;

namespace CursosIglesia.Models.DTOs
{
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

    public class Activity
    {
        public Guid IdActividad { get; set; }
        public Guid IdTema { get; set; }
        public int Tipo { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Instrucciones { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public int PuntosMaximos { get; set; }
        public bool MostrarRespuestas { get; set; }
        public bool PermitirEnvioTarde { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<ActivityQuestion> Questions { get; set; } = new();
    }

    public class ActivityQuestion
    {
        public Guid IdActivityQuestion { get; set; }
        public Guid IdActividad { get; set; }
        public string TextoPregunta { get; set; } = "";
        public int TipoPregunta { get; set; }
        public int Orden { get; set; }
        public bool EsObligatoria { get; set; }
        public int Puntos { get; set; }
        public List<string>? Opciones { get; set; }
        public string? RespuestaCorrecta { get; set; }
        public string? TextoFeedback { get; set; }
    }

    public class ActivityResponseDto
    {
        public Guid Id { get; set; }
        public Guid IdActividad { get; set; }
        public Guid IdUsuario { get; set; }
        public Guid IdPregunta { get; set; }
        public string TextoPregunta { get; set; } = "";
        public string Respuesta { get; set; } = "";
        public int? PuntosGanados { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string? Feedback { get; set; }
    }

    public class CreateActivityRequest
    {
        public Guid IdTema { get; set; }
        public int Tipo { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Instrucciones { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public int PuntosMaximos { get; set; } = 100;
        public bool MostrarRespuestas { get; set; }
        public bool PermitirEnvioTarde { get; set; }
        public List<CreateActivityQuestionRequest> Preguntas { get; set; } = new();
    }

    public class CreateActivityQuestionRequest
    {
        public string TextoPregunta { get; set; } = "";
        public int TipoPregunta { get; set; }
        public int Orden { get; set; }
        public bool EsObligatoria { get; set; }
        public int Puntos { get; set; }
        public List<string>? Opciones { get; set; }
        public string? RespuestaCorrecta { get; set; }
        public string? TextoFeedback { get; set; }
    }

    public class SubmitActivityResponseRequest
    {
        public Guid IdActividad { get; set; }
        public Dictionary<Guid, string> Respuestas { get; set; } = new();
    }

    public class StudentActivityResponseItem
    {
        public Guid IdUsuario { get; set; }
        public string NombreEstudiante { get; set; } = "";
        public string EmailEstudiante { get; set; } = "";
        public bool EstaCompletado { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int? TotalPuntosGanados { get; set; }
        public int PuntosMaximos { get; set; }
    }

    public class ActivityStatsDto
    {
        public Guid IdActividad { get; set; }
        public string TituloActividad { get; set; } = "";
        public int TotalEstudiantes { get; set; }
        public int RespuestasCompletadas { get; set; }
        public double PorcentajeCompletado { get; set; }
        public double PuntajePromedio { get; set; }
        public List<QuestionStatsDto> EstadisticasPreguntas { get; set; } = new();
    }

    public class QuestionStatsDto
    {
        public Guid IdPregunta { get; set; }
        public string TextoPregunta { get; set; } = "";
        public int TipoPregunta { get; set; }
        public int TotalRespuestas { get; set; }
        public double PromedioPuntos { get; set; }
        public Dictionary<string, int> FrecuenciaRespuestas { get; set; } = new();
    }

    public class AddFeedbackRequest
    {
        public Guid IdRespuesta { get; set; }
        public int PuntosGanados { get; set; }
        public string? Feedback { get; set; }
    }

    public class UpdateActivityRequest
    {
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Instrucciones { get; set; }
        public int Tipo { get; set; }
        public int PuntosMaximos { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public bool MostrarRespuestas { get; set; }
        public bool PermitirEnvioTarde { get; set; }
    }
}
