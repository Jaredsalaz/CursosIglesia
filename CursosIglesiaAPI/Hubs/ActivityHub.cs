using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CursosIglesia.Hubs;

[Authorize]
public class ActivityHub : Hub
{
    /// <summary>
    /// Notifica a los maestros cuando un estudiante envía una respuesta
    /// </summary>
    public async Task NotifyNewResponse(Guid activityId, Guid studentId, string studentName)
    {
        // Notificar al grupo del maestro del tema
        await Clients.Group($"activity-{activityId}-maestro")
            .SendAsync("NewResponseSubmitted", new
            {
                ActivityId = activityId,
                StudentId = studentId,
                StudentName = studentName,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifica a los estudiantes cuando el maestro publica feedback
    /// </summary>
    public async Task NotifyFeedbackAdded(Guid responseId, Guid studentId)
    {
        await Clients.User(studentId.ToString())
            .SendAsync("FeedbackReceived", new
            {
                ResponseId = responseId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Agrega cliente a grupo para notificaciones de actividad
    /// </summary>
    public async Task JoinActivityGroup(Guid activityId, string role)
    {
        var groupName = role == "Maestro" ? $"activity-{activityId}-maestro" : $"activity-{activityId}-students";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Remueve cliente del grupo
    /// </summary>
    public async Task LeaveActivityGroup(Guid activityId, string role)
    {
        var groupName = role == "Maestro" ? $"activity-{activityId}-maestro" : $"activity-{activityId}-students";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"[ActivityHub] Usuario conectado: {userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"[ActivityHub] Usuario desconectado: {userId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Propiedad helper para obtener ConnectionId
    private string Connection => Context?.ConnectionId ?? string.Empty;
}
