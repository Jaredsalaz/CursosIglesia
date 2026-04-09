using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IMaestroService
{
    Task<MaestroMetrics> GetMaestroMetricsAsync(Guid userId);
    Task<List<Course>> GetMaestroCoursesAsync(Guid userId);
    Task<List<Lesson>> GetLeccionesCursoAsync(Guid courseId);
    Task<ApiResponse<Guid>> CrearLeccionAsync(Guid userId, CreateLessonRequest request);
    Task<ApiResponse> ActualizarLeccionAsync(Guid lessonId, CreateLessonRequest request);
    Task<ApiResponse> EliminarLeccionAsync(Guid lessonId);
    
    // Temas
    Task<List<Tema>> GetTemasLeccionAsync(Guid lessonId);
    Task<ApiResponse<Guid>> CrearTemaAsync(Guid userId, CreateTemaRequest request);
    Task<ApiResponse> ActualizarTemaAsync(Guid temaId, CreateTemaRequest request);
    Task<ApiResponse> EliminarTemaAsync(Guid temaId);

    Task<List<EnrollmentDetail>> GetInscripcionesCursoAsync(Guid courseId);
    Task<List<EnrollmentDetail>> GetTodasMisInscripcionesAsync(Guid userId);
    Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid userId);
    Task<ApiResponse> SubirDocumentoAsync(Guid userId, TeacherDocument document);

    // Archivos de apoyo por tema
    Task<List<ArchivoTema>> GetArchivosTemaAsync(Guid temaId);
    Task<ApiResponse<Guid>> AgregarArchivoTemaAsync(ArchivoTema archivo);
    Task<ApiResponse<string>> EliminarArchivoTemaAsync(Guid archivoId);
}
