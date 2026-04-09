using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiMaestroService : IMaestroService
{
    private readonly HttpClient _httpClient;

    public ApiMaestroService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MaestroMetrics> GetMaestroMetricsAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<MaestroMetrics>("api/maestro/metrics") ?? new();
    }

    public async Task<List<Course>> GetMaestroCoursesAsync(Guid userId)
    {
        var courses = await _httpClient.GetFromJsonAsync<List<Course>>("api/maestro/courses") ?? new();
        courses.ForEach(FixUrls);
        return courses;
    }

    public async Task<List<Lesson>> GetLeccionesCursoAsync(Guid courseId)
    {
        return await _httpClient.GetFromJsonAsync<List<Lesson>>($"api/maestro/courses/{courseId}/lessons") ?? new();
    }

    public async Task<ApiResponse<Guid>> CrearLeccionAsync(Guid userId, CreateLessonRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/maestro/courses/{request.CourseId}/lessons", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>()
            ?? new ApiResponse<Guid> { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> ActualizarLeccionAsync(Guid lessonId, CreateLessonRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/maestro/lessons/{lessonId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> EliminarLeccionAsync(Guid lessonId)
    {
        var response = await _httpClient.DeleteAsync($"/api/maestro/lessons/{lessonId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<List<Tema>> GetTemasLeccionAsync(Guid lessonId)
    {
        return await _httpClient.GetFromJsonAsync<List<Tema>>($"/api/maestro/lessons/{lessonId}/topics") ?? new();
    }

    public async Task<ApiResponse<Guid>> CrearTemaAsync(Guid lessonId, CreateTemaRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/maestro/lessons/{lessonId}/topics", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>()
            ?? new ApiResponse<Guid> { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> ActualizarTemaAsync(Guid temaId, CreateTemaRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/maestro/topics/{temaId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> EliminarTemaAsync(Guid temaId)
    {
        var response = await _httpClient.DeleteAsync($"/api/maestro/topics/{temaId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<List<EnrollmentDetail>> GetInscripcionesCursoAsync(Guid courseId)
    {
        return await _httpClient.GetFromJsonAsync<List<EnrollmentDetail>>($"api/maestro/courses/{courseId}/enrollments") ?? new();
    }

    public async Task<List<EnrollmentDetail>> GetTodasMisInscripcionesAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<List<EnrollmentDetail>>("api/maestro/enrollments") ?? new();
    }

    public async Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<List<TeacherDocument>>("api/maestro/documents") ?? new();
    }

    public async Task<ApiResponse> SubirDocumentoAsync(Guid userId, TeacherDocument document)
    {
        var response = await _httpClient.PostAsJsonAsync("api/maestro/documents", document);
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<List<ArchivoTema>> GetArchivosTemaAsync(Guid temaId)
    {
        return await _httpClient.GetFromJsonAsync<List<ArchivoTema>>($"api/courses/topics/{temaId}/files") ?? new();
    }

    public async Task<ArchivoTema?> SubirArchivoTemaAsync(Guid temaId, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _httpClient.PostAsync($"api/upload/topic-file?temaId={temaId}", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<TopicFileUploadResponse>();
        if (json?.Success == true && json.Archivo != null)
        {
            // Fix relative URLs to absolute
            var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
            if (!string.IsNullOrEmpty(baseUrl) && json.Archivo.RutaArchivo.StartsWith("/"))
                json.Archivo.RutaArchivo = baseUrl + json.Archivo.RutaArchivo;

            return json.Archivo;
        }
        return null;
    }

    public async Task<bool> EliminarArchivoTemaAsync(Guid archivoId)
    {
        var response = await _httpClient.DeleteAsync($"api/upload/topic-file/{archivoId}");
        return response.IsSuccessStatusCode;
    }

    private void FixUrls(Course course)
    {
        if (course == null) return;
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl))
        {
            if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl.StartsWith("/"))
                course.ImageUrl = baseUrl + course.ImageUrl;
            if (!string.IsNullOrEmpty(course.InstructorImageUrl) && course.InstructorImageUrl.StartsWith("/"))
                course.InstructorImageUrl = baseUrl + course.InstructorImageUrl;
        }
    }

    // Helper response class for file upload
    private class TopicFileUploadResponse
    {
        public bool Success { get; set; }
        public ArchivoTema? Archivo { get; set; }
    }
}
