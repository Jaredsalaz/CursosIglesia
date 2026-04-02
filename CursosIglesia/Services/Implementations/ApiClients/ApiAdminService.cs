using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiAdminService : IAdminService
{
    private readonly HttpClient _httpClient;

    public ApiAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardMetrics> GetDashboardMetricsAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardMetrics>("api/admin/metrics") ?? new();
    }

    public async Task<List<MaestroDetail>> GetAllMaestrosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<MaestroDetail>>("api/admin/maestros") ?? new();
    }

    public async Task<ApiResponse<Guid>> CrearMaestroAsync(CreateMaestroRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/maestros", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() 
            ?? new ApiResponse<Guid> { Success = false, Message = "Error de conexión" };
    }

    public async Task<List<CourseWithMaestro>> GetAllCoursesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CourseWithMaestro>>("api/admin/courses") ?? new();
    }

    public async Task<ApiResponse<Guid>> CrearCursoAsync(CreateCourseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/courses", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() 
            ?? new ApiResponse<Guid> { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> AsignarCursoAsync(Guid courseId, Guid maestroId)
    {
        var response = await _httpClient.PutAsync($"api/admin/courses/{courseId}/assign/{maestroId}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse>() 
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<List<EnrollmentDetail>> GetAllInscripcionesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<EnrollmentDetail>>("api/admin/enrollments") ?? new();
    }

    public async Task<List<TeacherDocument>> GetAllDocumentosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TeacherDocument>>("api/admin/documents") ?? new();
    }

    public async Task<List<Category>> GetCategoriasAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Category>>("api/admin/categories") ?? new();
    }

    public async Task<ApiResponse> ActualizarCursoAsync(Guid courseId, UpdateCourseRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/admin/courses/{courseId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> EliminarCursoAsync(Guid courseId)
    {
        var response = await _httpClient.DeleteAsync($"api/admin/courses/{courseId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse<Guid>> CrearCategoriaAsync(CreateCategoryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/categories", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>()
            ?? new ApiResponse<Guid> { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> ActualizarCategoriaAsync(Guid id, CreateCategoryRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/admin/categories/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<ApiResponse> EliminarCategoriaAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/admin/categories/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>()
            ?? new ApiResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<FileUploadResponse> UploadImageAsync(MultipartFormDataContent content)
    {
        try
        {
            var response = await _httpClient.PostAsync("api/upload/image", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FileUploadResponse>()
                       ?? new FileUploadResponse { Success = false, Message = "Respuesta vacía" };
            }
            return new FileUploadResponse { Success = false, Message = $"Error HTTP: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new FileUploadResponse { Success = false, Message = $"Excepción: {ex.Message}" };
        }
    }
}
