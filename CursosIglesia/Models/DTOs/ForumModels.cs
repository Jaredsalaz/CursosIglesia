namespace CursosIglesia.Models.DTOs;

public class ForumDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? TemaId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int TotalPosts { get; set; }
    public int TotalParticipants { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ForumPostDto
{
    public Guid Id { get; set; }
    public Guid ForumId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public string? UserAvatar { get; set; }
    public Guid? ParentPostId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = "";
    public int Likes { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ForumPostDto> Replies { get; set; } = new();
}

public class CreateForumPostRequest
{
    public Guid ForumId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = "";
    public Guid? ParentPostId { get; set; }
}

public class CreateForumRequest
{
    public Guid CourseId { get; set; }
    public Guid? TemaId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class UpdateForumPostRequest
{
    public string Content { get; set; } = "";
}

public class UserActivityDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public int PostCount { get; set; }
    public int LikeCount { get; set; }
}

public class ForumEngagementStatsDto
{
    public Guid ForumId { get; set; }
    public string ForumTitle { get; set; } = "";
    public int TotalPosts { get; set; }
    public int TotalReplies { get; set; }
    public int UniqueParticipants { get; set; }
    public int TotalLikes { get; set; }
    public List<UserActivityDto> TopContributors { get; set; } = new();
}
