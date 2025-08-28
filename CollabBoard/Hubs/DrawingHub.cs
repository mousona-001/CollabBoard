using System;
using CollabBoard.Models;
using CollabBoard.Services;
using Microsoft.AspNetCore.SignalR;

namespace CollabBoard.Hubs;

public class DrawingHub : Hub
{
    private readonly IRoomService _roomService;
    private readonly ILogger<DrawingHub> _logger;

    public DrawingHub(IRoomService roomService, ILogger<DrawingHub> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    public async Task JoinRoom(string roomId)
    {
        try
        {
            if (!await _roomService.RoomExistsAsync(roomId))
            {
                await Clients.Caller.SendAsync("JoinRoomError", "Room does not exist");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await _roomService.AddUserToRoomAsync(roomId, Context.ConnectionId);

            // Send drawing history to the new user
            var history = await _roomService.GetDrawingHistoryAsync(roomId);
            await Clients.Caller.SendAsync("DrawingHistory", history);

            // Notify others that a user joined
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", Context.ConnectionId);

            _logger.LogInformation(
                "User {ConnectionId} joined room {RoomId}",
                Context.ConnectionId,
                roomId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room {RoomId}", roomId);
            await Clients.Caller.SendAsync("JoinRoomError", "Failed to join room");
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await _roomService.RemoveUserFromRoomAsync(roomId, Context.ConnectionId);

            // Notify others that user left
            await Clients.OthersInGroup(roomId).SendAsync("UserLeft", Context.ConnectionId);

            _logger.LogInformation(
                "User {ConnectionId} left room {RoomId}",
                Context.ConnectionId,
                roomId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
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

            // Set the user ID and timestamp
            drawingEvent.UserId = Context.ConnectionId;
            drawingEvent.Timestamp = DateTime.UtcNow;

            // Store the drawing event
            await _roomService.AddDrawingEventAsync(roomId, drawingEvent);

            // Broadcast to all users in the room (including sender for confirmation)
            await Clients.Group(roomId).SendAsync("ReceiveDrawingEvent", drawingEvent);

            _logger.LogDebug(
                "Drawing event sent in room {RoomId}: {Type} at ({X}, {Y})",
                roomId,
                drawingEvent.Type,
                drawingEvent.X,
                drawingEvent.Y
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending drawing event in room {RoomId}", roomId);
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

            _logger.LogInformation(
                "Canvas cleared in room {RoomId} by user {ConnectionId}", roomId, Context.ConnectionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing canvas in room {RoomId}", roomId);
            await Clients.Caller.SendAsync("DrawingError", "Failed to clear canvas");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Note: In a production app, you'd want to track which rooms the user was in
            // For simplicity, we'll let the cleanup happen when LeaveRoom is called explicitly
            _logger.LogInformation("User {ConnectionId} disconnected", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
