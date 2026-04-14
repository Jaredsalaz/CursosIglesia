using CursosIglesia.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CursosIglesia.Services.Interfaces;

public interface ITestimonialService
{
    Task<List<Testimonial>> GetTestimonialsAsync(int count = 6);
    Task<List<Testimonial>> GetByCourseIdAsync(Guid courseId, bool onlyApproved = true);
    Task<List<Testimonial>> GetPendingTestimonialsAsync();
    Task<bool> AddTestimonialAsync(Guid userId, Guid courseId, string comment, int rating);
    Task<bool> ApproveTestimonialAsync(Guid testimonialId);
}
