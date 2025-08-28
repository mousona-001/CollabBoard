using System;
using System.ComponentModel.DataAnnotations;

namespace CollabBoard.Models;

public class CreateRoomResponse
{
    public string RoomId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class JoinRoomRequest
{
    [Required]
    public string RoomId { get; set; } = string.Empty;
}

public class JoinRoomResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DrawingEvent>? DrawingHistory { get; set; }
}
