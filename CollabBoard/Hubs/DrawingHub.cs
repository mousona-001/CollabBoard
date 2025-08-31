using CollabBoard.Models;
using CollabBoard.Services;
using Microsoft.AspNetCore.SignalR;

namespace CollabBoard.Hubs
{
    public class DrawingHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<DrawingHub> _logger;
        private static readonly Dictionary<string, string> _connectionToRoom = new();
        private static readonly Dictionary<string, string> _connectionToUserName = new();
        private static readonly Dictionary<string, bool> _connectionDrawingStatus = new();

        public DrawingHub(IRoomService roomService, ILogger<DrawingHub> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        public async Task JoinRoom(string roomId, string userName)
        {
            try
            {
                if (!await _roomService.RoomExistsAsync(roomId))
                {
                    await Clients.Caller.SendAsync("JoinRoomError", "Room does not exist");
                    return;
                }

                // Track connection info
                _connectionToRoom[Context.ConnectionId] = roomId;
                _connectionToUserName[Context.ConnectionId] = userName;
                _connectionDrawingStatus[Context.ConnectionId] = false;

                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await _roomService.AddUserToRoomAsync(roomId, Context.ConnectionId);

                // Send drawing history to the new user
                var history = await _roomService.GetDrawingHistoryAsync(roomId);
                await Clients.Caller.SendAsync("DrawingHistory", history);

                // Get current users in room and broadcast updated list
                var connectedUsers = GetConnectedUsersInRoom(roomId);
                await Clients.Group(roomId).SendAsync("UsersUpdated", connectedUsers);

                // Notify others that a user joined
                await Clients
                    .OthersInGroup(roomId)
                    .SendAsync(
                        "UserJoined",
                        new { ConnectionId = Context.ConnectionId, UserName = userName }
                    );

                _logger.LogInformation(
                    $"User {userName} ({Context.ConnectionId}) joined room {roomId}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining room {roomId}");
                await Clients.Caller.SendAsync("JoinRoomError", "Failed to join room");
            }
        }

        public async Task LeaveRoom(string roomId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await _roomService.RemoveUserFromRoomAsync(roomId, Context.ConnectionId);

                // Remove from tracking
                _connectionToRoom.Remove(Context.ConnectionId);
                _connectionToUserName.Remove(Context.ConnectionId);
                _connectionDrawingStatus.Remove(Context.ConnectionId);

                // Get updated user list and broadcast to remaining users
                var connectedUsers = GetConnectedUsersInRoom(roomId);
                await Clients.Group(roomId).SendAsync("UsersUpdated", connectedUsers);

                // Notify others that user left
                await Clients.OthersInGroup(roomId).SendAsync("UserLeft", Context.ConnectionId);

                _logger.LogInformation($"User {Context.ConnectionId} left room {roomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving room {roomId}");
            }
        }

        public async Task SendDrawingEvent(string roomId, DrawingEvent drawingEvent)
        {
            try
            {
                if (!await _roomService.RoomExistsAsync(roomId))
                {
                    await Clients.Caller.SendAsync("DrawingError", "Room does not exist");
                    return;
                }

                // Get user info from tracking
                var userName = _connectionToUserName.GetValueOrDefault(
                    Context.ConnectionId,
                    "Unknown"
                );

                // Set the user ID, name and timestamp
                drawingEvent.UserId = Context.ConnectionId;
                drawingEvent.UserName = userName;
                drawingEvent.Timestamp = DateTime.UtcNow;

                // Update drawing status and notify
                if (drawingEvent.Type == "startDrawing")
                {
                    _connectionDrawingStatus[Context.ConnectionId] = true;
                    await Clients
                        .Group(roomId)
                        .SendAsync(
                            "UserStartedDrawing",
                            new
                            {
                                UserId = Context.ConnectionId,
                                UserName = userName,
                                Tool = drawingEvent.Tool ?? "pen",
                            }
                        );

                    // Update users list
                    var connectedUsers = GetConnectedUsersInRoom(roomId);
                    await Clients.Group(roomId).SendAsync("UsersUpdated", connectedUsers);
                }
                else if (drawingEvent.Type == "stopDrawing")
                {
                    _connectionDrawingStatus[Context.ConnectionId] = false;
                    await Clients
                        .Group(roomId)
                        .SendAsync(
                            "UserStoppedDrawing",
                            new { UserId = Context.ConnectionId, UserName = userName }
                        );

                    // Update users list
                    var connectedUsers = GetConnectedUsersInRoom(roomId);
                    await Clients.Group(roomId).SendAsync("UsersUpdated", connectedUsers);
                }

                // Store the drawing event
                await _roomService.AddDrawingEventAsync(roomId, drawingEvent);

                // Broadcast to all users in the room
                await Clients.Group(roomId).SendAsync("ReceiveDrawingEvent", drawingEvent);

                var toolInfo = !string.IsNullOrEmpty(drawingEvent.Tool)
                    ? $" using {drawingEvent.Tool}"
                    : "";
                _logger.LogDebug(
                    $"Drawing event sent in room {roomId} by {userName}: {drawingEvent.Type}{toolInfo}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending drawing event in room {roomId}");
                await Clients.Caller.SendAsync("DrawingError", "Failed to send drawing event");
            }
        }

        public async Task ClearCanvas(string roomId)
        {
            try
            {
                if (!await _roomService.RoomExistsAsync(roomId))
                {
                    await Clients.Caller.SendAsync("DrawingError", "Room does not exist");
                    return;
                }

                // Clear the drawing history in the room
                var room = await _roomService.GetRoomAsync(roomId);
                if (room != null)
                {
                    room.DrawingHistory.Clear();
                }

                // Broadcast clear canvas event to all users in the room
                await Clients.Group(roomId).SendAsync("ClearCanvas");

                var userName = _connectionToUserName.GetValueOrDefault(
                    Context.ConnectionId,
                    "Unknown"
                );
                _logger.LogInformation($"Canvas cleared in room {roomId} by {userName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing canvas in room {roomId}");
                await Clients.Caller.SendAsync("DrawingError", "Failed to clear canvas");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                // Check if this connection was in a room
                if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomId))
                {
                    await LeaveRoom(roomId);
                }

                _logger.LogInformation($"User {Context.ConnectionId} disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling disconnection");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Helper method to get connected users in a room from our tracking dictionaries
        private List<ConnectedUser> GetConnectedUsersInRoom(string roomId)
        {
            var users = new List<ConnectedUser>();

            foreach (var kvp in _connectionToRoom)
            {
                if (kvp.Value == roomId)
                {
                    var connectionId = kvp.Key;
                    var userName = _connectionToUserName.GetValueOrDefault(connectionId, "Unknown");
                    var isDrawing = _connectionDrawingStatus.GetValueOrDefault(connectionId, false);

                    users.Add(
                        new ConnectedUser
                        {
                            ConnectionId = connectionId,
                            UserName = userName,
                            JoinedAt = DateTime.UtcNow, // We could track this too if needed
                            IsDrawing = isDrawing,
                        }
                    );
                }
            }

            return users;
        }
    }
}
