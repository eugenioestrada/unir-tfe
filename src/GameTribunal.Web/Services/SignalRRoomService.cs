using GameTribunal.Application.DTOs;
using GameTribunal.Application.Services;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GameTribunal.Web.Services;

/// <summary>
/// SignalR-aware wrapper for RoomService that broadcasts updates to connected clients.
/// Implements RF-011 (real-time synchronization).
/// </summary>
public sealed class SignalRRoomService
{
    private readonly RoomService _roomService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<SignalRRoomService> _logger;

    public SignalRRoomService(
        RoomService roomService,
        IHubContext<GameHub> hubContext,
        ILogger<SignalRRoomService> logger)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a room. Delegates to RoomService.
    /// </summary>
    public Task<RoomDto> CreateRoomAsync(GameMode mode, CancellationToken cancellationToken = default)
    {
        return _roomService.CreateRoomAsync(mode, cancellationToken);
    }

    /// <summary>
    /// Adds a player to a room and broadcasts the updated state via SignalR (RF-011).
    /// </summary>
    public async Task<RoomDto> JoinRoomAsync(string roomCode, string alias, CancellationToken cancellationToken = default)
    {
        var roomDto = await _roomService.JoinRoomAsync(roomCode, alias, cancellationToken);

        _logger.LogInformation("Player joined room {RoomCode}. Broadcasting state update via SignalR.", roomCode);

        // Broadcast updated room state to all clients in the room group (RF-011)
        await _hubContext.Clients.Group(roomCode).SendAsync("RoomStateUpdated", roomDto, cancellationToken);

        return roomDto;
    }

    /// <summary>
    /// Generates the room URL for QR code.
    /// </summary>
    public string GenerateRoomUrl(string roomCode, string baseUrl)
    {
        return _roomService.GenerateRoomUrl(roomCode, baseUrl);
    }

    /// <summary>
    /// Gets a room by its code. Returns null if not found (RF-017).
    /// </summary>
    public Task<RoomDto?> GetRoomByCodeAsync(GameTribunal.Domain.ValueObjects.RoomCode roomCode, CancellationToken cancellationToken = default)
    {
        return _roomService.GetRoomByCodeAsync(roomCode, cancellationToken);
    }
}
