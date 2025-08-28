using System;
using CollabBoard.Models;

namespace CollabBoard.Services;

public interface IRoomService
{
    Task<string> CreateRoomAsync();
    Task<bool> RoomExistsAsync(string roomId);
    Task<Room?> GetRoomAsync(string roomId);
    Task AddUserToRoomAsync(string roomId, string userId);
    Task RemoveUserFromRoomAsync(string roomId, string userId);
    Task AddDrawingEventAsync(string roomId, DrawingEvent drawingEvent);
    Task<List<DrawingEvent>> GetDrawingHistoryAsync(string roomId);
}
