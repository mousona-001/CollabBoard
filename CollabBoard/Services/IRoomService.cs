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
    Task UpdateUserNameAsync(string roomId, string userId, string userName);
    Task SetUserDrawingStatusAsync(string roomId, string userId, bool isDrawing);
    Task<List<ConnectedUser>> GetConnectedUsersAsync(string roomId);
    Task AddDrawingEventAsync(string roomId, DrawingEvent drawingEvent);
    Task<List<DrawingEvent>> GetDrawingHistoryAsync(string roomId);
}
