using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost("enroll/{courseId}")]
    public async Task<IActionResult> Enroll(Guid courseId)
    {
        var success = await _enrollmentService.EnrollAsync(courseId);
        return success ? Ok() : BadRequest();
    }

    [HttpPost("unenroll/{courseId}")]
    public async Task<IActionResult> Unenroll(Guid courseId)
    {
        var success = await _enrollmentService.UnenrollAsync(courseId);
        return success ? Ok() : BadRequest();
    }

    [HttpGet("check/{courseId}")]
    public async Task<ActionResult<bool>> IsEnrolled(Guid courseId)
    {
        var enrolled = await _enrollmentService.IsEnrolledAsync(courseId);
        return Ok(enrolled);
    }

    [HttpGet]
    public async Task<ActionResult<List<Enrollment>>> GetEnrollments()
    {
        var enrollments = await _enrollmentService.GetEnrollmentsAsync();
        return Ok(enrollments);
    }

    [HttpGet("{courseId}")]
    public async Task<ActionResult<Enrollment>> GetEnrollment(Guid courseId)
    {
        var enrollment = await _enrollmentService.GetEnrollmentAsync(courseId);
        if (enrollment == null) return NotFound();
        return Ok(enrollment);
    }

    [HttpPost("complete-lesson")]
    public async Task<IActionResult> CompleteLesson([FromBody] LessonUpdateProgressRequest request)
    {
        var success = await _enrollmentService.CompleteLessonAsync(request);
        return success ? Ok() : BadRequest();
    }

    [HttpPost("current-lesson/{courseId}/{lessonId}")]
    public async Task<IActionResult> SetCurrentLesson(Guid courseId, Guid lessonId)
    {
        var success = await _enrollmentService.SetCurrentLessonAsync(courseId, lessonId);
        return success ? Ok() : BadRequest();
    }
}
