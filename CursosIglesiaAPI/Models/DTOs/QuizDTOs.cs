using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

// -------- Create / Update --------

public class CreateQuizRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int MinAprobado { get; set; } = 70;
    public List<CreatePreguntaRequest> Preguntas { get; set; } = new();
}

public class CreatePreguntaRequest
{
    public TipoPregunta TipoPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Orden { get; set; }
    public List<CreateOpcionRequest> Opciones { get; set; } = new();
}

public class CreateOpcionRequest
{
    public string TextoOpcion { get; set; } = string.Empty;
    public string LetraOpcion { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
    public string ParEmparejar { get; set; } = string.Empty;
}

// -------- AI Generation --------

public class GenerateQuizRequest
{
    public string TextoContenido { get; set; } = string.Empty; // Lesson text content
    public int NumeroPreguntasDeseadas { get; set; } = 5;
}

// -------- Gemini AI Response Parsing --------

public class GeminiQuizResponse
{
    public List<GeminiPregunta> Preguntas { get; set; } = new();
}

public class GeminiPregunta
{
    public string Texto { get; set; } = string.Empty;
    public List<GeminiOpcion> Opciones { get; set; } = new();
}

public class GeminiOpcion
{
    public string Letra { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
}
