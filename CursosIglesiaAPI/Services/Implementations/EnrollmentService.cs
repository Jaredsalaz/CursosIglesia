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
        return enrollments.ToList();
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

    public async Task<bool> SetCurrentLessonAsync(Guid courseId, Guid lessonId)
    {
        // Mock for now until specific SP action is added
        return await Task.FromResult(true);
    }
}
