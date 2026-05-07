using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

// ===== Create Forum Request =====
public class CreateForumRequest
{
    public Guid CourseId { get; set; }
    public Guid? TemaId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ===== Create Forum Post Request =====
public class CreateForumPostRequest
{
    public Guid ForumId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentPostId { get; set; }
}

// ===== Forum Post Response DTO =====
public class ForumPostDto
{
    public Guid Id { get; set; }
    public Guid ForumId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public Guid? ParentPostId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ForumPostDto> Replies { get; set; } = new();
}

// ===== Forum DTO =====
public class ForumDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? TemaId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalPosts { get; set; }
    public int TotalParticipants { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== Forum Post Engagement Stats =====
public class ForumEngagementStatsDto
{
    public Guid ForumId { get; set; }
    public string ForumTitle { get; set; } = string.Empty;
    public int TotalPosts { get; set; }
    public int TotalReplies { get; set; }
    public int UniqueParticipants { get; set; }
    public int TotalLikes { get; set; }
    public List<UserActivityDto> TopContributors { get; set; } = new();
}

// ===== User Activity in Forum =====
public class UserActivityDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int LikeCount { get; set; }
}

// ===== Update Forum Post Request =====
public class UpdateForumPostRequest
{
    public string Content { get; set; } = string.Empty;
}

// ===== Pin/Unpin Forum Post Request =====
public class PinForumPostRequest
{
    public Guid PostId { get; set; }
    public bool IsPinned { get; set; }
}
