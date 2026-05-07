using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CursosIglesia.Hubs;

[Authorize]
public class ForumHub : Hub
{
    /// <summary>
    /// Notifica a todos en el foro cuando hay un nuevo post
    /// </summary>
    public async Task NotifyNewPost(Guid forumId, Guid postId, string authorName, string content)
    {
        await Clients.Group($"forum-{forumId}")
            .SendAsync("NewPostAdded", new
            {
                PostId = postId,
                ForumId = forumId,
                AuthorName = authorName,
                Content = content,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifica cuando alguien da like a un post
    /// </summary>
    public async Task NotifyPostLiked(Guid postId, int newLikeCount)
    {
        var groupName = $"post-{postId}-observers";
        await Clients.Group(groupName)
            .SendAsync("PostLiked", new
            {
                PostId = postId,
                NewLikeCount = newLikeCount,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifica cuando se elimina un post
    /// </summary>
    public async Task NotifyPostDeleted(Guid postId, Guid forumId)
    {
        await Clients.Group($"forum-{forumId}")
            .SendAsync("PostDeleted", new
            {
                PostId = postId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifica cuando se fija/desfija un post
    /// </summary>
    public async Task NotifyPostPinned(Guid postId, Guid forumId, bool isPinned)
    {
        await Clients.Group($"forum-{forumId}")
            .SendAsync("PostPinStatusChanged", new
            {
                PostId = postId,
                IsPinned = isPinned,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Agrega cliente a grupo para notificaciones del foro
    /// </summary>
    public async Task JoinForum(Guid forumId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"forum-{forumId}");
        Console.WriteLine($"[ForumHub] Usuario {Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value} se unió al foro {forumId}");
    }

    /// <summary>
    /// Remueve cliente del grupo del foro
    /// </summary>
    public async Task LeaveForum(Guid forumId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"forum-{forumId}");
        Console.WriteLine($"[ForumHub] Usuario {Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value} salió del foro {forumId}");
    }

    /// <summary>
    /// Agrega cliente a observadores de un post específico
    /// </summary>
    public async Task WatchPost(Guid postId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}-observers");
    }

    /// <summary>
    /// Remueve cliente de observadores del post
    /// </summary>
    public async Task UnwatchPost(Guid postId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}-observers");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"[ForumHub] Usuario conectado: {userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"[ForumHub] Usuario desconectado: {userId}");
        if (exception != null)
            Console.WriteLine($"[ForumHub] Excepción: {exception.Message}");
        await base.OnDisconnectedAsync(exception);
    }
}
