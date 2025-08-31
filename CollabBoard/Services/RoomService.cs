using System.Collections.Concurrent;
using CollabBoard.Models;

namespace CollabBoard.Services
{
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
                if (!room.ConnectedUsers.ContainsKey(userId))
                {
                    room.ConnectedUsers.Add(
                        userId,
                        new ConnectedUser
                        {
                            ConnectionId = userId,
                            UserName = "Unknown", // Will be updated by Hub
                            JoinedAt = DateTime.UtcNow,
                        }
                    );
                }
            }
            return Task.CompletedTask;
        }

        public Task RemoveUserFromRoomAsync(string roomId, string userId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.ConnectedUsers.Remove(userId);

                // Optional: Clean up empty rooms
                if (room.ConnectedUsers.Count == 0)
                {
                    // Could implement room cleanup logic here
                }
            }
            return Task.CompletedTask;
        }

        public Task UpdateUserNameAsync(string roomId, string userId, string userName)
        {
            if (
                _rooms.TryGetValue(roomId, out var room)
                && room.ConnectedUsers.TryGetValue(userId, out var user)
            )
            {
                user.UserName = userName;
            }
            return Task.CompletedTask;
        }

        public Task SetUserDrawingStatusAsync(string roomId, string userId, bool isDrawing)
        {
            if (
                _rooms.TryGetValue(roomId, out var room)
                && room.ConnectedUsers.TryGetValue(userId, out var user)
            )
            {
                user.IsDrawing = isDrawing;
            }
            return Task.CompletedTask;
        }

        public Task<List<ConnectedUser>> GetConnectedUsersAsync(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                return Task.FromResult(room.ConnectedUsers.Values.ToList());
            }
            return Task.FromResult(new List<ConnectedUser>());
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
}
