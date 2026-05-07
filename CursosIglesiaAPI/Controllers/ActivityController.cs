using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // POST api/activities — Create activity (Maestro only)
    [HttpPost]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateActivity([FromBody] CreateActivityRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var response = await _activityService.CreateActivityAsync(userId, request);
        return Ok(response);
    }

    // GET api/activities/{id} — Get activity details
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> GetActivity(Guid id)
    {
        var activity = await _activityService.GetActivityAsync(id);
        if (activity == null) return NotFound();
        return Ok(activity);
    }

    // GET api/topics/{temaId}/activities — List activities by tema
    [HttpGet("/api/topics/{temaId:guid}/activities")]
    [Authorize]
    public async Task<ActionResult<List<Activity>>> GetActivitiesByTema(Guid temaId)
    {
        var activities = await _activityService.GetActivitiesByTemaAsync(temaId);
        return Ok(activities);
    }

    // PUT api/activities/{id} — Update activity (Maestro only)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> UpdateActivity(Guid id, [FromBody] CreateActivityRequest request)
    {
        var response = await _activityService.UpdateActivityAsync(id, request);
        return Ok(response);
    }

    // DELETE api/activities/{id} — Delete activity (Maestro only)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteActivity(Guid id)
    {
        var response = await _activityService.DeleteActivityAsync(id);
        return Ok(response);
    }

    // POST api/activities/submit-response — Submit activity responses (Student)
    [HttpPost("submit-response")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> SubmitResponses([FromBody] SubmitActivityResponseRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var response = await _activityService.SubmitResponseAsync(userId, request);
        return Ok(response);
    }

    // GET api/activities/{id}/responses — Get all responses (Maestro only)
    [HttpGet("{id:guid}/responses")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<List<ActivityResponseDto>>> GetActivityResponses(Guid id)
    {
        var responses = await _activityService.GetActivityResponsesAsync(id);
        return Ok(responses);
    }

    // GET api/activities/{id}/student/{userId} — Get specific student response
    [HttpGet("{id:guid}/student/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<ActivityResponseDto>>> GetStudentResponse(Guid id, Guid userId)
    {
        var response = await _activityService.GetStudentResponseAsync(userId, id);
        return Ok(response);
    }

    // GET api/activities/{id}/my-response — Get student's own response
    [HttpGet("{id:guid}/my-response")]
    [Authorize]
    public async Task<ActionResult<List<ActivityResponseDto>>> GetMyResponse(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var responses = await _activityService.GetStudentResponseAsync(userId, id);
        return Ok(responses);
    }

    // GET api/activities/{id}/submissions — Get all student submissions (Maestro only)
    [HttpGet("{id:guid}/submissions")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<List<StudentActivityResponseItem>>> GetStudentSubmissions(Guid id)
    {
        var submissions = await _activityService.GetStudentSubmissionsAsync(id);
        return Ok(submissions);
    }

    // POST api/activities/feedback — Add feedback (Maestro only)
    [HttpPost("feedback")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> AddFeedback([FromBody] AddFeedbackRequest request)
    {
        var response = await _activityService.AddFeedbackAsync(request);
        return Ok(response);
    }

    // GET api/activities/{id}/stats — Get activity statistics (Maestro only)
    [HttpGet("{id:guid}/stats")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ActivityStatsDto>> GetActivityStats(Guid id)
    {
        var stats = await _activityService.GetActivityStatsAsync(id);
        if (stats == null) return NotFound();
        return Ok(stats);
    }

    // GET api/activities/course/{courseId}/ungraded — Check if student has ungraded activities
    [HttpGet("course/{courseId:guid}/ungraded")]
    [Authorize]
    public async Task<ActionResult<bool>> HasUngradedActivities(Guid courseId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var hasUngraded = await _activityService.HasUngradedActivitiesAsync(userId, courseId);
        return Ok(hasUngraded);
    }
}
