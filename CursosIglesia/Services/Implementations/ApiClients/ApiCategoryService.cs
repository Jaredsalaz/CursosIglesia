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
        var categories = await _httpClient.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
        categories.ForEach(FixUrls);
        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
        if (category != null) FixUrls(category);
        return category;
    }

    private void FixUrls(Category category)
    {
        if (category == null) return;
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(category.ImageUrl) && category.ImageUrl.StartsWith("/"))
        {
            category.ImageUrl = baseUrl + category.ImageUrl;
        }
    }
}
