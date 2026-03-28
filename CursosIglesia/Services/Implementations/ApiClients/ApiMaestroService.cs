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

    public async Task<List<Course>> GetMaestroCoursesAsync(Guid maestroId)
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>($"api/maestro/courses/{maestroId}") ?? new();
    }

    public async Task<Guid> CrearCursoAsync(Course course)
    {
        var response = await _httpClient.PostAsJsonAsync("api/maestro/courses", course);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task ActualizarCursoAsync(Course course)
    {
        await _httpClient.PutAsJsonAsync($"api/maestro/courses/{course.Id}", course);
    }

    public async Task SubirDocumentoAsync(TeacherDocument document)
    {
        await _httpClient.PostAsJsonAsync("api/maestro/documents", document);
    }

    public async Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid maestroId)
    {
        return await _httpClient.GetFromJsonAsync<List<TeacherDocument>>($"api/maestro/documents/{maestroId}") ?? new();
    }
}
