using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IActivityService
{
    // CRUD de Actividades
    Task<ApiResponse<Guid>> CreateActivityAsync(Guid userId, CreateActivityRequest request);
    Task<Activity?> GetActivityAsync(Guid activityId);
    Task<List<Activity>> GetActivitiesByTemaAsync(Guid temaId);
    Task<ApiResponse> UpdateActivityAsync(Guid activityId, CreateActivityRequest request);
    Task<ApiResponse> DeleteActivityAsync(Guid activityId);

    // Respuestas de Estudiantes
    Task<ApiResponse> SubmitResponseAsync(Guid userId, SubmitActivityResponseRequest request);
    Task<List<ActivityResponseDto>> GetActivityResponsesAsync(Guid activityId);
    Task<List<ActivityResponseDto>> GetStudentResponseAsync(Guid userId, Guid activityId);
    Task<List<StudentActivityResponseItem>> GetStudentSubmissionsAsync(Guid activityId);
    Task<ApiResponse> AddFeedbackAsync(AddFeedbackRequest request);

    // Análisis para Maestro
    Task<ActivityStatsDto?> GetActivityStatsAsync(Guid activityId);

    Task<bool> HasUngradedActivitiesAsync(Guid userId, Guid courseId);
}
