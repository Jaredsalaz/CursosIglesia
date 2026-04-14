using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
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

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<List<Testimonial>>> GetByCourse(Guid courseId)
    {
        var testimonials = await _testimonialService.GetByCourseIdAsync(courseId);
        return Ok(testimonials);
    }

    [HttpGet("pending")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<Testimonial>>> GetPending()
    {
        var testimonials = await _testimonialService.GetPendingTestimonialsAsync();
        return Ok(testimonials);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> AddTestimonial([FromBody] AddTestimonialRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var success = await _testimonialService.AddTestimonialAsync(userId, request.CourseId, request.Comment, request.Rating);
        if (success) return Ok(new { Message = "Testimonio enviado para revisión." });
        
        return BadRequest(new { Message = "No se pudo enviar el testimonio." });
    }

    [HttpPost("{id}/approve")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult> Approve(Guid id)
    {
        var success = await _testimonialService.ApproveTestimonialAsync(id);
        if (success) return Ok(new { Message = "Testimonio aprobado." });
        return NotFound();
    }
}
