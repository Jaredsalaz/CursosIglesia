using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaestroController : ControllerBase
{
    private readonly IMaestroService _maestroService;
    private readonly IAuthService _authService;

    public MaestroController(IMaestroService maestroService, IAuthService authService)
    {
        _maestroService = maestroService;
        _authService = authService;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<Course>>> GetMyCourses()
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        var courses = await _maestroService.GetMaestroCoursesAsync(_authService.CurrentUser.Id);
        return Ok(courses);
    }

    [HttpPost("courses")]
    public async Task<ActionResult<Guid>> CreateCourse([FromBody] Course course)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        // Ensure the course is associated with the current instructor
        course.InstructorId = _authService.CurrentUser.Id;
        var newCourseId = await _maestroService.CrearCursoAsync(course);
        return Ok(newCourseId);
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
    public async Task<ActionResult> UploadDocument([FromBody] TeacherDocument document)
    {
        if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            return Unauthorized();

        document.IdMaestro = _authService.CurrentUser.Id;
        await _maestroService.SubirDocumentoAsync(document);
        return Ok(new { Success = true, Message = "Documento subido correctamente" });
    }
}
