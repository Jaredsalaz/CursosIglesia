using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/testimonials")]
public class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;

    public TestimonialsController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Testimonial>>> GetTestimonials([FromQuery] int count = 6)
    {
        var testimonials = await _testimonialService.GetTestimonialsAsync(count);
        return Ok(testimonials);
    }
}
