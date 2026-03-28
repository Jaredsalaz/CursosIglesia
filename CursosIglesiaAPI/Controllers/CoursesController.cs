using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetAllCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<List<Course>>> GetFeaturedCourses()
    {
        var courses = await _courseService.GetFeaturedCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("popular/{count}")]
    public async Task<ActionResult<List<Course>>> GetPopularCourses(int count)
    {
        var courses = await _courseService.GetPopularCoursesAsync(count);
        return Ok(courses);
    }

    [HttpGet("recent/{count}")]
    public async Task<ActionResult<List<Course>>> GetRecentCourses(int count)
    {
        var courses = await _courseService.GetRecentCoursesAsync(count);
        return Ok(courses);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<List<Course>>> GetCoursesByCategory(Guid categoryId)
    {
        var courses = await _courseService.GetCoursesByCategoryAsync(categoryId);
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<Course>>> SearchCourses([FromBody] CourseSearchRequest request)
    {
        var courses = await _courseService.SearchCoursesAsync(request);
        return Ok(courses);
    }
}
