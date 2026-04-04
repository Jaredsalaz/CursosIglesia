namespace CursosIglesia.Models;

public class Quiz
{
    public Guid IdQuiz { get; set; }
    public Guid IdTema { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int MinAprobado { get; set; } = 70;
    public List<Pregunta> Preguntas { get; set; } = new();
}

public class Pregunta
{
    public Guid IdPregunta { get; set; }
    public Guid IdQuiz { get; set; }
    public TipoPregunta TipoPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Orden { get; set; }
    public List<OpcionPregunta> Opciones { get; set; } = new();
}

public class OpcionPregunta
{
    public Guid IdOpcion { get; set; }
    public Guid IdPregunta { get; set; }
    public string TextoOpcion { get; set; } = string.Empty;
    public string LetraOpcion { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
    public string ParEmparejar { get; set; } = string.Empty;
}

public enum TipoPregunta
{
    OpcionMultiple = 0,
    Emparejar = 1
}

// ---- DTOs for creating/editing ----

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

public class GenerateQuizRequest
{
    public string TextoContenido { get; set; } = string.Empty;
}
