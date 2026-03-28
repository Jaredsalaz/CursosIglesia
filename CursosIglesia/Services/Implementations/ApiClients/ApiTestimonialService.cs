using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiTestimonialService : ITestimonialService
{
    private readonly HttpClient _httpClient;
    public ApiTestimonialService(HttpClient httpClient) => _httpClient = httpClient;
    public async Task<List<Testimonial>> GetTestimonialsAsync(int count = 5) => await _httpClient.GetFromJsonAsync<List<Testimonial>>($"api/testimonials?count={count}") ?? new();
}
