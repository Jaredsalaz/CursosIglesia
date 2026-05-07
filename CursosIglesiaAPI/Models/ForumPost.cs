namespace CursosIglesia.Models;

public class ForumPost
{
    public Guid Id { get; set; }
    public Guid ForumId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ForumLike> Likers { get; set; } = new();
}
