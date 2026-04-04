using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class QuizService : IQuizService
{
    private readonly string _conn;
    private readonly GeminiService _gemini;

    public QuizService(IConfiguration configuration, GeminiService gemini)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
        _gemini = gemini;
    }

    public async Task<Quiz?> GetQuizByTemaAsync(Guid temaId)
    {
        using IDbConnection db = new SqlConnection(_conn);
        const string sql = @"
            SELECT q.*, p.*, o.*
            FROM Quizzes q
            LEFT JOIN Preguntas p ON p.IdQuiz = q.IdQuiz
            LEFT JOIN OpcionesPregunta o ON o.IdPregunta = p.IdPregunta
            WHERE q.IdTema = @IdTema
            ORDER BY p.Orden, o.LetraOpcion";

        var quizDict = new Dictionary<Guid, Quiz>();
        var pregDict = new Dictionary<Guid, Pregunta>();

        await db.QueryAsync<Quiz, Pregunta, OpcionPregunta, Quiz>(
            sql,
            (q, p, o) =>
            {
                if (!quizDict.TryGetValue(q.IdQuiz, out var quiz))
                {
                    quiz = q;
                    quiz.Preguntas = new List<Pregunta>();
                    quizDict[q.IdQuiz] = quiz;
                }
                if (p != null && p.IdPregunta != Guid.Empty)
                {
                    if (!pregDict.TryGetValue(p.IdPregunta, out var preg))
                    {
                        preg = p;
                        preg.Opciones = new List<OpcionPregunta>();
                        pregDict[p.IdPregunta] = preg;
                        quiz.Preguntas.Add(preg);
                    }
                    if (o != null && o.IdOpcion != Guid.Empty)
                        preg.Opciones.Add(o);
                }
                return quiz;
            },
            new { IdTema = temaId },
            splitOn: "IdPregunta,IdOpcion"
        );

        return quizDict.Values.FirstOrDefault();
    }

    public async Task<Quiz> CreateQuizAsync(Guid temaId, CreateQuizRequest req)
    {
        using IDbConnection db = new SqlConnection(_conn);
        var quizId = Guid.NewGuid();
        db.Open();
        using var tx = ((SqlConnection)db).BeginTransaction();
        try
        {
            await db.ExecuteAsync(
                @"INSERT INTO Quizzes (IdQuiz, IdTema, Titulo, Descripcion, MinAprobado)
                  VALUES (@IdQuiz, @IdTema, @Titulo, @Descripcion, @MinAprobado)",
                new { IdQuiz = quizId, IdTema = temaId, req.Titulo, req.Descripcion, req.MinAprobado },
                transaction: tx);

            foreach (var p in req.Preguntas)
            {
                var pregId = Guid.NewGuid();
                await db.ExecuteAsync(
                    @"INSERT INTO Preguntas (IdPregunta, IdQuiz, TipoPregunta, Texto, Orden)
                      VALUES (@IdPregunta, @IdQuiz, @TipoPregunta, @Texto, @Orden)",
                    new { IdPregunta = pregId, IdQuiz = quizId, TipoPregunta = (int)p.TipoPregunta, p.Texto, p.Orden },
                    transaction: tx);

                foreach (var o in p.Opciones)
                {
                    await db.ExecuteAsync(
                        @"INSERT INTO OpcionesPregunta (IdOpcion, IdPregunta, TextoOpcion, LetraOpcion, EsCorrecta, ParEmparejar)
                          VALUES (NEWID(), @IdPregunta, @TextoOpcion, @LetraOpcion, @EsCorrecta, @ParEmparejar)",
                        new { IdPregunta = pregId, o.TextoOpcion, o.LetraOpcion, o.EsCorrecta, o.ParEmparejar },
                        transaction: tx);
                }
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }

        return (await GetQuizByTemaAsync(temaId))!;
    }

    public async Task<Quiz> UpdateQuizAsync(Guid quizId, CreateQuizRequest req)
    {
        using IDbConnection db = new SqlConnection(_conn);

        // Get temaId first
        var temaId = await db.QueryFirstOrDefaultAsync<Guid>(
            "SELECT IdTema FROM Quizzes WHERE IdQuiz = @Id", new { Id = quizId });

        // Delete and recreate (simpler than deep diff)
        await db.ExecuteAsync("DELETE FROM Quizzes WHERE IdQuiz = @Id", new { Id = quizId });
        return await CreateQuizAsync(temaId, req);
    }

    public async Task<bool> DeleteQuizAsync(Guid quizId)
    {
        using IDbConnection db = new SqlConnection(_conn);
        var rows = await db.ExecuteAsync("DELETE FROM Quizzes WHERE IdQuiz = @Id", new { Id = quizId });
        return rows > 0;
    }

    public async Task<Quiz> GenerateQuizWithAIAsync(Guid temaId, string contenidoLeccion)
    {
        var aiResponse = await _gemini.GenerateQuizQuestionsAsync(contenidoLeccion, 5);

        // Convert AI response to create request
        var req = new CreateQuizRequest
        {
            Titulo = "Quiz generado con IA",
            Descripcion = "Preguntas generadas automáticamente por inteligencia artificial.",
            MinAprobado = 70,
            Preguntas = aiResponse.Preguntas.Select((p, i) => new CreatePreguntaRequest
            {
                TipoPregunta = TipoPregunta.OpcionMultiple,
                Texto = p.Texto,
                Orden = i + 1,
                Opciones = p.Opciones.Select(o => new CreateOpcionRequest
                {
                    TextoOpcion = o.Texto,
                    LetraOpcion = o.Letra,
                    EsCorrecta = o.EsCorrecta
                }).ToList()
            }).ToList()
        };

        // Delete existing quiz for this tema if any
        using IDbConnection db = new SqlConnection(_conn);
        var existingId = await db.QueryFirstOrDefaultAsync<Guid?>(
            "SELECT IdQuiz FROM Quizzes WHERE IdTema = @Id", new { Id = temaId });
        if (existingId.HasValue)
            await DeleteQuizAsync(existingId.Value);

        return await CreateQuizAsync(temaId, req);
    }
}
