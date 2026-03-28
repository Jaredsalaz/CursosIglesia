using CursosIglesia.Models;

namespace CursosIglesia.Services.Interfaces;

public interface ITestimonialService
{
    Task<List<Testimonial>> GetTestimonialsAsync(int count = 5);
}
