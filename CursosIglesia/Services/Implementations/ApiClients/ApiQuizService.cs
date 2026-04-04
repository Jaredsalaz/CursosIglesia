using CursosIglesia.Models;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiQuizService
{
    private readonly HttpClient _http;

    public ApiQuizService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Quiz?> GetQuizByTemaAsync(Guid temaId)
    {
        try
        {
            return await _http.GetFromJsonAsync<Quiz>($"api/quiz/tema/{temaId}");
        }
        catch { return null; }
    }

    public async Task<Quiz?> CreateQuizAsync(Guid temaId, CreateQuizRequest request)
    {
        var resp = await _http.PostAsJsonAsync($"api/quiz/tema/{temaId}", request);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Quiz>();
    }

    public async Task<Quiz?> UpdateQuizAsync(Guid quizId, CreateQuizRequest request)
    {
        var resp = await _http.PutAsJsonAsync($"api/quiz/{quizId}", request);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Quiz>();
    }

    public async Task<bool> DeleteQuizAsync(Guid quizId)
    {
        var resp = await _http.DeleteAsync($"api/quiz/{quizId}");
        return resp.IsSuccessStatusCode;
    }

    public async Task<Quiz?> GenerateWithAIAsync(Guid temaId, string textContent)
    {
        var resp = await _http.PostAsJsonAsync($"api/quiz/tema/{temaId}/generate",
            new GenerateQuizRequest { TextoContenido = textContent });
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Quiz>();
    }
}
