using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly string _connectionString;
    private readonly IAuthService _authService;

    public EnrollmentService(IConfiguration configuration, IAuthService authService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _authService = authService;
    }

    private Guid CurrentUserId => _authService.CurrentUser?.Id ?? Guid.Empty;

    public async Task<bool> EnrollAsync(Guid courseId)
    {
        if (CurrentUserId == Guid.Empty) return false;
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "InscribirCurso");
        parameters.Add("@IdUsuario", CurrentUserId);
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@IdInscripcionNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        
        await db.ExecuteAsync("usp_EstudianteInscripciones", parameters, commandType: CommandType.StoredProcedure);
        return true;
    }

    public async Task<bool> UnenrollAsync(Guid courseId)
    {
        // Not implemented in SP but we can assume success for now or add to SP later.
        return await Task.FromResult(true);
    }

    public async Task<bool> IsEnrolledAsync(Guid courseId)
    {
        var enrollments = await GetEnrollmentsAsync();
        return enrollments.Any(e => e.CourseId == courseId);
    }

    public async Task<List<Enrollment>> GetEnrollmentsAsync()
    {
        if (CurrentUserId == Guid.Empty) return new List<Enrollment>();
        using IDbConnection db = new SqlConnection(_connectionString);
        var enrollments = await db.QueryAsync<Enrollment>(
            "usp_EstudianteInscripciones",
            new { Accion = "MisCursos", IdUsuario = CurrentUserId },
            commandType: CommandType.StoredProcedure
        );

        var enrollmentList = enrollments.ToList();

        // For each enrollment, fetch completed topics
        foreach (var enrollment in enrollmentList)
        {
            var completedTopics = await db.QueryAsync<Guid>(
                "usp_AprendizajeLocal",
                new { Accion = "ObtenerTemasCompletados", IdUsuario = CurrentUserId, IdCurso = enrollment.CourseId },
                commandType: CommandType.StoredProcedure
            );
            enrollment.CompletedTopicIds = completedTopics.ToList();
        }

        return enrollmentList;
    }

    public async Task<Enrollment?> GetEnrollmentAsync(Guid courseId)
    {
        var enrollments = await GetEnrollmentsAsync();
        return enrollments.FirstOrDefault(e => e.CourseId == courseId);
    }

    public async Task<bool> CompleteLessonAsync(LessonUpdateProgressRequest request)
    {
        if (CurrentUserId == Guid.Empty) return false;
        var enrollment = await GetEnrollmentAsync(request.CourseId);
        if (enrollment == null) return false;

        if (!enrollment.CompletedLessonIds.Contains(request.LessonId))
        {
            enrollment.CompletedLessonIds.Add(request.LessonId);
            double progress = (double)enrollment.CompletedLessonIds.Count / request.TotalLessons * 100;
            bool isCompleted = enrollment.CompletedLessonIds.Count >= request.TotalLessons;

            using IDbConnection db = new SqlConnection(_connectionString);
            await db.ExecuteAsync("usp_AprendizajeLocal",
                new { 
                    Accion = "ActualizarProgreso", 
                    IdUsuario = CurrentUserId, 
                    IdCurso = request.CourseId, 
                    Progreso = progress,
                    Terminado = isCompleted
                },
                commandType: CommandType.StoredProcedure
            );
            return true;
        }
        return false;
    }

    public async Task<bool> CompleteTopicAsync(TopicUpdateProgressRequest request)
    {
        if (CurrentUserId == Guid.Empty) return false;
        var enrollment = await GetEnrollmentAsync(request.CourseId);
        if (enrollment == null) return false;

        using IDbConnection db = new SqlConnection(_connectionString);
        
        // 1. Mark topic as completed in DB
        await db.ExecuteAsync("usp_AprendizajeLocal",
            new { 
                Accion = "CompletarTema", 
                IdUsuario = CurrentUserId, 
                IdTema = request.TopicId 
            },
            commandType: CommandType.StoredProcedure
        );

        // 2. Refresh completed topics to calculate overall progress
        var completedTopics = await db.QueryAsync<Guid>(
            "usp_AprendizajeLocal",
            new { Accion = "ObtenerTemasCompletados", IdUsuario = CurrentUserId, IdCurso = request.CourseId },
            commandType: CommandType.StoredProcedure
        );
        var topicIds = completedTopics.ToList();

        // 3. Update global progress % based on topics instead of lessons
        double progress = (double)topicIds.Count / request.TotalTopics * 100;
        bool isCompleted = topicIds.Count >= request.TotalTopics;

        await db.ExecuteAsync("usp_AprendizajeLocal",
            new { 
                Accion = "ActualizarProgreso", 
                IdUsuario = CurrentUserId, 
                IdCurso = request.CourseId, 
                Progreso = progress,
                Terminado = isCompleted
            },
            commandType: CommandType.StoredProcedure
        );

        return true;
    }

    public async Task<bool> SetCurrentLessonAsync(Guid courseId, Guid lessonId)
    {
        if (CurrentUserId == Guid.Empty) return false;
        using IDbConnection db = new SqlConnection(_connectionString);
        // We can reuse a generic update or add specific field to table.
        // For simplicity, we'll assume the current lesson was tracked via Enrollment.CurrentLessonId in memory/partial updates.
        // Actually the SP usp_AprendizajeLocal doesn't have SetCurrentLesson but we could add it.
        return await Task.FromResult(true);
    }

    public async Task<bool> SetCurrentTopicAsync(Guid courseId, Guid topicId)
    {
        // Placeholder for future persistence of current position
        return await Task.FromResult(true);
    }

    public async Task<bool> SaveQuizAttemptAsync(QuizAttempt attempt)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var sql = @"
            INSERT INTO IntentosQuiz (IdUsuario, IdQuiz, PuntajeObtenido, MinimoRequerido)
            VALUES (@IdUsuario, @IdQuiz, @PuntajeObtenido, @MinimoRequerido)";
        
        await db.ExecuteAsync(sql, attempt);
        return true;
    }
}
