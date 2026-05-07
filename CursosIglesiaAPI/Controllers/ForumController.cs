using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/forums")]
public class ForumController : ControllerBase
{
    private readonly IForumService _forumService;

    public ForumController(IForumService forumService)
    {
        _forumService = forumService;
    }

    // POST api/forums — Create forum (Maestro only)
    [HttpPost]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateForum([FromBody] CreateForumRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var response = await _forumService.CreateForumAsync(userId, request);
        return Ok(response);
    }

    // GET api/forums/{id} — Get forum details
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<Forum>> GetForum(Guid id)
    {
        var forum = await _forumService.GetForumAsync(id);
        if (forum == null) return NotFound();
        return Ok(forum);
    }

    // GET api/forums/course/{courseId} — List course forums
    [HttpGet("course/{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<ForumDto>>> GetCourseForums(Guid courseId)
    {
        var forums = await _forumService.GetCourseForumsAsync(courseId);
        return Ok(forums);
    }

    // DELETE api/forums/{id} — Delete forum (Maestro only)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteForum(Guid id)
    {
        var response = await _forumService.DeleteForumAsync(id);
        return Ok(response);
    }

    // POST api/forums/posts — Create post
    [HttpPost("posts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<Guid>>> CreatePost([FromBody] CreateForumPostRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var response = await _forumService.CreatePostAsync(userId, request);
        return Ok(response);
    }

    // GET api/forums/{forumId}/posts — Get forum posts
    [HttpGet("{forumId:guid}/posts")]
    [Authorize]
    public async Task<ActionResult<List<ForumPostDto>>> GetForumPosts(Guid forumId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var posts = await _forumService.GetForumPostsAsync(forumId, userId);
        return Ok(posts);
    }

    // GET api/forums/posts/{postId} — Get single post
    [HttpGet("posts/{postId:guid}")]
    [Authorize]
    public async Task<ActionResult<ForumPostDto>> GetPost(Guid postId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var post = await _forumService.GetPostAsync(postId, userId);
        if (post == null) return NotFound();
        return Ok(post);
    }

    // PUT api/forums/posts/{postId} — Update post
    [HttpPut("posts/{postId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> UpdatePost(Guid postId, [FromBody] UpdateForumPostRequest request)
    {
        var response = await _forumService.UpdatePostAsync(postId, request);
        return Ok(response);
    }

    // DELETE api/forums/posts/{postId} — Delete post (Owner or Maestro)
    [HttpDelete("posts/{postId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeletePost(Guid postId)
    {
        var response = await _forumService.DeletePostAsync(postId);
        return Ok(response);
    }

    // POST api/forums/posts/{postId}/pin — Pin post (Maestro only)
    [HttpPut("posts/{postId:guid}/pin")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> PinPost(Guid postId, [FromBody] PinForumPostRequest request)
    {
        var response = await _forumService.PinPostAsync(postId, request.IsPinned);
        return Ok(response);
    }

    // POST api/forums/posts/{postId}/like — Like post
    [HttpPost("posts/{postId:guid}/like")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> LikePost(Guid postId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var response = await _forumService.LikePostAsync(userId, postId);
        return Ok(response);
    }

    // DELETE api/forums/posts/{postId}/like/{userId} — Unlike post
    [HttpDelete("posts/{postId:guid}/like/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> UnlikePost(Guid postId, Guid userId)
    {
        var response = await _forumService.UnlikePostAsync(userId, postId);
        return Ok(response);
    }

    // GET api/forums/{forumId}/stats — Get forum statistics (Maestro only)
    [HttpGet("{forumId:guid}/stats")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<ForumEngagementStatsDto>> GetForumStats(Guid forumId)
    {
        var stats = await _forumService.GetForumStatsAsync(forumId);
        if (stats == null) return NotFound();
        return Ok(stats);
    }
}
