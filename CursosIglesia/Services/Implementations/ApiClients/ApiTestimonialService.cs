using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using System.Net.Http.Json;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class ApiTestimonialService : ITestimonialService
{
    private readonly HttpClient _httpClient;
    public ApiTestimonialService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<Testimonial>> GetTestimonialsAsync(int count = 6) 
        => await _httpClient.GetFromJsonAsync<List<Testimonial>>($"api/testimonials?count={count}") ?? new();

    public async Task<List<Testimonial>> GetByCourseIdAsync(Guid courseId)
        => await _httpClient.GetFromJsonAsync<List<Testimonial>>($"api/testimonials/course/{courseId}") ?? new();

    public async Task<List<Testimonial>> GetPendingTestimonialsAsync()
        => await _httpClient.GetFromJsonAsync<List<Testimonial>>("api/testimonials/pending") ?? new();

    public async Task<bool> AddTestimonialAsync(Guid courseId, string comment, int rating)
    {
        var response = await _httpClient.PostAsJsonAsync("api/testimonials", new { CourseId = courseId, Comment = comment, Rating = rating });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveTestimonialAsync(Guid testimonialId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/testimonials/{testimonialId}/approve", new { });
        return response.IsSuccessStatusCode;
    }
}
