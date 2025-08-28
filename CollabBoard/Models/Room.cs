using System;

namespace CollabBoard.Models;

public class Room
{
    public string RoomId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public HashSet<string> ConnectedUsers { get; set; } = new HashSet<string>();
    public List<DrawingEvent> DrawingHistory { get; set; } = new List<DrawingEvent>();
}
