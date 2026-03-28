using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiCategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;

    public ApiCategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
    }
}
