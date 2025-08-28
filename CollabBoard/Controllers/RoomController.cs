using System;
using CollabBoard.Models;
using CollabBoard.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollabBoard.Controllers;

[Route("api/room")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomController> _logger;

    public RoomController(IRoomService roomService, ILogger<RoomController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateRoomResponse>> CreateRoom()
    {
        try
        {
            var roomId = await _roomService.CreateRoomAsync();
            _logger.LogInformation("Created room: {RoomId}", roomId);

            return Ok(
                new CreateRoomResponse { RoomId = roomId, Message = "Room created successfully" }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            return StatusCode(500, new CreateRoomResponse { Message = "Failed to create room" });
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult<JoinRoomResponse>> JoinRoom([FromBody] JoinRoomRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new JoinRoomResponse { Success = false, Message = "Invalid room ID" }
                );
            }

            var roomExists = await _roomService.RoomExistsAsync(request.RoomId);
            if (!roomExists)
            {
                return NotFound(
                    new JoinRoomResponse { Success = false, Message = "Room not found" }
                );
            }

            var drawingHistory = await _roomService.GetDrawingHistoryAsync(request.RoomId);

            return Ok(
                new JoinRoomResponse
                {
                    Success = true,
                    Message = "Room found successfully",
                    DrawingHistory = drawingHistory,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room {RoomId}", request.RoomId);
            return StatusCode(
                500,
                new JoinRoomResponse { Success = false, Message = "Failed to join room" }
            );
        }
    }

    [HttpGet("{roomId}/exists")]
    public async Task<ActionResult<bool>> RoomExists(string roomId)
    {
        try
        {
            var exists = await _roomService.RoomExistsAsync(roomId);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if room exists: {RoomId}", roomId);
            return StatusCode(500, false);
        }
    }
}
