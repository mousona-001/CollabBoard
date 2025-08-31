using System;

namespace CollabBoard.Models;

public class Room
{
    public string RoomId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, ConnectedUser> ConnectedUsers { get; set; } =
        new Dictionary<string, ConnectedUser>();
    public List<DrawingEvent> DrawingHistory { get; set; } = new List<DrawingEvent>();
}
