using CursosIglesia.Models;

namespace CursosIglesia.Services.Interfaces;

public interface ITestimonialService
{
    Task<List<Testimonial>> GetTestimonialsAsync(int count = 6);
    Task<List<Testimonial>> GetByCourseIdAsync(Guid courseId);
    Task<List<Testimonial>> GetPendingTestimonialsAsync();
    Task<bool> AddTestimonialAsync(Guid courseId, string comment, int rating);
    Task<bool> ApproveTestimonialAsync(Guid testimonialId);
}
