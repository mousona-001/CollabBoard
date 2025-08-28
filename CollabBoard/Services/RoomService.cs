using System;
using System.Collections.Concurrent;
using CollabBoard.Models;

namespace CollabBoard.Services;

public class RoomService : IRoomService
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly Random _random = new();

    public Task<string> CreateRoomAsync()
    {
        string roomId;
        do
        {
            roomId = GenerateRoomId();
        } while (_rooms.ContainsKey(roomId));

        var room = new Room { RoomId = roomId, CreatedAt = DateTime.UtcNow };

        _rooms.TryAdd(roomId, room);
        return Task.FromResult(roomId);
    }

    public Task<bool> RoomExistsAsync(string roomId)
    {
        return Task.FromResult(_rooms.ContainsKey(roomId));
    }

    public Task<Room?> GetRoomAsync(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return Task.FromResult(room);
    }

    public Task AddUserToRoomAsync(string roomId, string userId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.ConnectedUsers.Add(userId);
        }
        return Task.CompletedTask;
    }

    public Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.ConnectedUsers.Remove(userId);

            // Optional: Remove empty rooms after some time
            if (room.ConnectedUsers.Count == 0)
            {
                // You might want to implement a cleanup mechanism here
                // For now, we'll keep the room for rejoining
            }
        }
        return Task.CompletedTask;
    }

    public Task AddDrawingEventAsync(string roomId, DrawingEvent drawingEvent)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.DrawingHistory.Add(drawingEvent);
        }
        return Task.CompletedTask;
    }

    public Task<List<DrawingEvent>> GetDrawingHistoryAsync(string roomId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            return Task.FromResult(room.DrawingHistory);
        }
        return Task.FromResult(new List<DrawingEvent>());
    }

    private string GenerateRoomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(
            Enumerable.Repeat(chars, 6).Select(s => s[_random.Next(s.Length)]).ToArray()
        );
    }
}
