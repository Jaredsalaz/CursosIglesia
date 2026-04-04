using System.Text;
using System.Text.Json;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Implementations;

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(IConfiguration configuration)
    {
        _http = new HttpClient();
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<GeminiQuizResponse> GenerateQuizQuestionsAsync(string contenidoLeccion, int numPreguntas = 5)
    {
        var jsonFormat = @"{
  ""preguntas"": [
    {
      ""texto"": ""¿Cuál es la pregunta?"",
      ""opciones"": [
        { ""letra"": ""A"", ""texto"": ""Primera opción"", ""esCorrecta"": false },
        { ""letra"": ""B"", ""texto"": ""Segunda opción"", ""esCorrecta"": true },
        { ""letra"": ""C"", ""texto"": ""Tercera opción"", ""esCorrecta"": false },
        { ""letra"": ""D"", ""texto"": ""Cuarta opción"", ""esCorrecta"": false }
      ]
    }
  ]
}";
        var prompt = $"Eres un experto en educación religiosa y catequesis. " +
            $"Basándote ÚNICAMENTE en el siguiente contenido de una lección, " +
            $"genera exactamente {numPreguntas} preguntas de opción múltiple (A, B, C, D) en español.\n\n" +
            "REGLAS:\n" +
            "- Cada pregunta debe tener exactamente 4 opciones (A, B, C, D)\n" +
            "- Solo una opción debe ser correcta\n" +
            "- Las preguntas deben ser claras, concisas y directamente relacionadas con el contenido\n" +
            "- Responde SOLAMENTE con un JSON válido, sin texto adicional, sin markdown, sin explicaciones\n\n" +
            $"FORMATO REQUERIDO:\n{jsonFormat}\n\n" +
            $"CONTENIDO DE LA LECCIÓN:\n{contenidoLeccion}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = 8192,
                responseMimeType = "application/json"
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpResponse = await _http.PostAsync(url, content);
        
        // Fallback for 503 High Demand on Preview models
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var fallbackUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={_apiKey}";
            httpResponse = await _http.PostAsync(fallbackUrl, content);
        }

        var responseStr = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                throw new Exception("Los servidores de Inteligencia Artificial están muy ocupados en este momento. Por favor, intenta de nuevo en un minuto.");
            
            throw new Exception($"Error de IA: {responseStr}");
        }

        // Parse Gemini's wrapper
        using var doc = JsonDocument.Parse(responseStr);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Sanitize in case the model ignored responseMimeType and returned markdown
        text = text.Trim();
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            text = text.Substring(7);
        else if (text.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            text = text.Substring(3);
        if (text.EndsWith("```"))
            text = text.Substring(0, text.Length - 3);
        text = text.Trim();

        Console.WriteLine($"[Gemini Response Length]: {text.Length}");

        // Parse the quiz JSON from the text
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var quizResponse = JsonSerializer.Deserialize<GeminiQuizResponse>(text, options) ?? new GeminiQuizResponse();
        return quizResponse;
    }
}
