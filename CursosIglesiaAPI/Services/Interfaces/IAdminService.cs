using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IAdminService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync();
    Task<List<MaestroDetail>> GetAllMaestrosAsync();
    Task<ApiResponse<Guid>> CrearMaestroAsync(CreateMaestroRequest request);
    Task<List<CourseWithMaestro>> GetAllCoursesAsync();
    Task<ApiResponse<Guid>> CrearCursoAsync(CreateCourseRequest request);
    Task<ApiResponse> ActualizarCursoAsync(Guid courseId, UpdateCourseRequest request);
    Task<ApiResponse> EliminarCursoAsync(Guid courseId);
    Task<ApiResponse> AsignarCursoAsync(Guid courseId, Guid maestroId);
    Task<List<EnrollmentDetail>> GetAllInscripcionesAsync();
    Task<List<TeacherDocument>> GetAllDocumentosAsync();
    Task<List<Category>> GetCategoriasAsync();
    Task<ApiResponse<Guid>> CrearCategoriaAsync(string nombre, string? descripcion, string? icono, string? imagenUrl);
    Task<ApiResponse> ActualizarCategoriaAsync(Guid id, string? nombre, string? descripcion, string? icono, string? imagenUrl);
    Task<ApiResponse> EliminarCategoriaAsync(Guid id);
}
