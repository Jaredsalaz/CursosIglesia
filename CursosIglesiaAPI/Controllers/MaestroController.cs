using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Maestro,SuperAdmin")]
public class MaestroController : ControllerBase
{
    private readonly IMaestroService _maestroService;
    private readonly IAuthService _authService;

    public MaestroController(IMaestroService maestroService, IAuthService authService)
    {
        _maestroService = maestroService;
        _authService = authService;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<MaestroMetrics>> GetMetrics()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var metrics = await _maestroService.GetMaestroMetricsAsync(_authService.CurrentUser.Id);
        return Ok(metrics);
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<Course>>> GetMyCourses()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var courses = await _maestroService.GetMaestroCoursesAsync(_authService.CurrentUser.Id);
        return Ok(courses);
    }

    [HttpGet("courses/{courseId}/lessons")]
    public async Task<ActionResult<List<Lesson>>> GetCourseLessons(Guid courseId)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var lessons = await _maestroService.GetLeccionesCursoAsync(courseId);
        return Ok(lessons);
    }

    [HttpPost("courses/{courseId}/lessons")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateLesson(Guid courseId, [FromBody] CreateLessonRequest request)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        request.CourseId = courseId;
        var result = await _maestroService.CrearLeccionAsync(_authService.CurrentUser.Id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("lessons/{lessonId}")]
    public async Task<ActionResult<ApiResponse>> UpdateLesson(Guid lessonId, [FromBody] CreateLessonRequest request)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var result = await _maestroService.ActualizarLeccionAsync(lessonId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("lessons/{lessonId}")]
    public async Task<ActionResult<ApiResponse>> DeleteLesson(Guid lessonId)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var result = await _maestroService.EliminarLeccionAsync(lessonId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("lessons/{lessonId}/topics")]
    public async Task<ActionResult<List<Tema>>> GetLessonTopics(Guid lessonId)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var topics = await _maestroService.GetTemasLeccionAsync(lessonId);
        return Ok(topics);
    }

    [HttpPost("lessons/{lessonId}/topics")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTopic(Guid lessonId, [FromBody] CreateTemaRequest request)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        request.LessonId = lessonId;
        var result = await _maestroService.CrearTemaAsync(_authService.CurrentUser.Id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("topics/{topicId}")]
    public async Task<ActionResult<ApiResponse>> UpdateTopic(Guid topicId, [FromBody] CreateTemaRequest request)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var result = await _maestroService.ActualizarTemaAsync(topicId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("topics/{topicId}")]
    public async Task<ActionResult<ApiResponse>> DeleteTopic(Guid topicId)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var result = await _maestroService.EliminarTemaAsync(topicId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("courses/{courseId}/enrollments")]
    public async Task<ActionResult<List<EnrollmentDetail>>> GetCourseEnrollments(Guid courseId)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var enrollments = await _maestroService.GetInscripcionesCursoAsync(courseId);
        return Ok(enrollments);
    }

    [HttpGet("enrollments")]
    public async Task<ActionResult<List<EnrollmentDetail>>> GetAllMyEnrollments()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var enrollments = await _maestroService.GetTodasMisInscripcionesAsync(_authService.CurrentUser.Id);
        return Ok(enrollments);
    }

    [HttpGet("documents")]
    public async Task<ActionResult<List<TeacherDocument>>> GetMyDocuments()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var documents = await _maestroService.GetDocumentosMaestroAsync(_authService.CurrentUser.Id);
        return Ok(documents);
    }

    [HttpPost("documents")]
    public async Task<ActionResult<ApiResponse>> UploadDocument([FromBody] TeacherDocument document)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var result = await _maestroService.SubirDocumentoAsync(_authService.CurrentUser.Id, document);
        return Ok(result);
    }
}
