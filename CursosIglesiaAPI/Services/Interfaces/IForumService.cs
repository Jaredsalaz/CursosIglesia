using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IForumService
{
    // CRUD de Foros
    Task<ApiResponse<Guid>> CreateForumAsync(Guid userId, CreateForumRequest request);
    Task<Forum?> GetForumAsync(Guid forumId);
    Task<List<ForumDto>> GetCourseForumsAsync(Guid courseId);
    Task<ApiResponse> DeleteForumAsync(Guid forumId);

    // Gestión de Posts
    Task<ApiResponse<Guid>> CreatePostAsync(Guid userId, CreateForumPostRequest request);
    Task<List<ForumPostDto>> GetForumPostsAsync(Guid forumId, Guid? currentUserId = null);
    Task<ForumPostDto?> GetPostAsync(Guid postId, Guid? currentUserId = null);
    Task<ApiResponse> UpdatePostAsync(Guid postId, UpdateForumPostRequest request);
    Task<ApiResponse> DeletePostAsync(Guid postId);
    Task<ApiResponse> PinPostAsync(Guid postId, bool isPinned);

    // Interacciones
    Task<ApiResponse> LikePostAsync(Guid userId, Guid postId);
    Task<ApiResponse> UnlikePostAsync(Guid userId, Guid postId);

    // Estadísticas
    Task<ForumEngagementStatsDto?> GetForumStatsAsync(Guid forumId);
}
