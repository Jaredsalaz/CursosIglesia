using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiDailyVerseService : IDailyVerseService
{
    private readonly HttpClient _httpClient;

    public ApiDailyVerseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DailyVerseDTO?> GetDailyVerseAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DailyVerseDTO>("api/daily-verse");
        }
        catch (Exception)
        {
            return null;
        }
    }
}
