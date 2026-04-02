using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<DashboardMetrics>> GetMetrics()
    {
        var metrics = await _adminService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("maestros")]
    public async Task<ActionResult<List<MaestroDetail>>> GetMaestros()
    {
        var maestros = await _adminService.GetAllMaestrosAsync();
        return Ok(maestros);
    }

    [HttpPost("maestros")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMaestro([FromBody] CreateMaestroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new ApiResponse<Guid> { Success = false, Message = "Nombre, email y contraseña son requeridos" });

        var result = await _adminService.CrearMaestroAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<CourseWithMaestro>>> GetCourses()
    {
        var courses = await _adminService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpPost("courses")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new ApiResponse<Guid> { Success = false, Message = "El título del curso es requerido" });

        var result = await _adminService.CrearCursoAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("courses/{id}")]
    public async Task<ActionResult<ApiResponse>> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var result = await _adminService.ActualizarCursoAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("courses/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCourse(Guid id)
    {
        var result = await _adminService.EliminarCursoAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("courses/{courseId}/assign/{maestroId}")]
    public async Task<ActionResult<ApiResponse>> AssignCourse(Guid courseId, Guid maestroId)
    {
        var result = await _adminService.AsignarCursoAsync(courseId, maestroId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("enrollments")]
    public async Task<ActionResult<List<EnrollmentDetail>>> GetEnrollments()
    {
        var enrollments = await _adminService.GetAllInscripcionesAsync();
        return Ok(enrollments);
    }

    [HttpGet("documents")]
    public async Task<ActionResult<List<TeacherDocument>>> GetDocuments()
    {
        var documents = await _adminService.GetAllDocumentosAsync();
        return Ok(documents);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<Category>>> GetCategories()
    {
        var categories = await _adminService.GetCategoriasAsync();
        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiResponse<Guid> { Success = false, Message = "El nombre es requerido" });

        var result = await _adminService.CrearCategoriaAsync(request.Nombre, request.Descripcion, request.Icono, request.ImagenUrl);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("categories/{id}")]
    public async Task<ActionResult<ApiResponse>> UpdateCategory(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var result = await _adminService.ActualizarCategoriaAsync(id, request.Nombre, request.Descripcion, request.Icono, request.ImagenUrl);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("categories/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCategory(Guid id)
    {
        var result = await _adminService.EliminarCategoriaAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
