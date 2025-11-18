using GameTribunal.Application.Contracts;
using GameTribunal.Application.DTOs;
using GameTribunal.Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace GameTribunal.Web.Hubs;

/// <summary>
/// SignalR hub for real-time game room communication.
/// Implements RF-011 (real-time synchronization) and RF-012 (reconnection support).
/// </summary>
public sealed class GameHub : Hub
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<GameHub> _logger;
    
    // Static dictionary to track player sessions: playerId -> (roomCode, connectionId)
    private static readonly Dictionary<Guid, (string RoomCode, string ConnectionId)> _playerSessions = new();
    private static readonly object _sessionLock = new();

    public GameHub(IRoomRepository roomRepository, ILogger<GameHub> logger)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Called when a player joins a room. Adds them to the SignalR group and tracks their session.
    /// </summary>
    /// <param name="roomCode">The room code.</param>
    /// <param name="playerId">The player's unique ID.</param>
    public async Task JoinRoom(string roomCode, Guid playerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomCode, nameof(roomCode));
        
        _logger.LogInformation("Player {PlayerId} joining room {RoomCode} with connection {ConnectionId}.", 
            playerId, roomCode, Context.ConnectionId);

        // Add to SignalR group for room-based messaging
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Track player session for reconnection (RF-012)
        lock (_sessionLock)
        {
            if (_playerSessions.TryGetValue(playerId, out var existingSession))
            {
                _logger.LogInformation("Player {PlayerId} reconnecting to room {RoomCode}. Previous connection: {OldConnectionId}, New connection: {NewConnectionId}.",
                    playerId, roomCode, existingSession.ConnectionId, Context.ConnectionId);
            }
            
            _playerSessions[playerId] = (roomCode, Context.ConnectionId);
        }

        // Broadcast updated room state to all players in the room (RF-011)
        await BroadcastRoomState(roomCode);
    }

    /// <summary>
    /// Broadcasts the current room state to all connected players in the room.
    /// Implements RF-011 (real-time synchronization).
    /// </summary>
    /// <param name="roomCode">The room code.</param>
    public async Task BroadcastRoomState(string roomCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomCode, nameof(roomCode));

        if (!RoomCode.TryFrom(roomCode, out var code))
        {
            _logger.LogWarning("Invalid room code format: {RoomCode}.", roomCode);
            return;
        }

        var room = await _roomRepository.GetByCodeAsync(code);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomCode} not found when broadcasting state.", roomCode);
            return;
        }

        var roomDto = new RoomDto(
            room.Code.Value,
            room.Mode,
            room.Players.Select(p => new PlayerDto(p.Id, p.Alias, p.ConnectionStatus)).ToList(),
            room.CanStartGame()
        );

        _logger.LogInformation("Broadcasting room state for {RoomCode} to all connected clients. Player count: {PlayerCount}.",
            roomCode, roomDto.Players.Count);

        // Send to all clients in the room group
        await Clients.Group(roomCode).SendAsync("RoomStateUpdated", roomDto);
    }

    /// <summary>
    /// Called when a client disconnects. Logs the event for monitoring.
    /// Player session is retained to support reconnection (RF-012).
    /// </summary>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Connection {ConnectionId} disconnected. Exception: {Exception}.",
            Context.ConnectionId, exception?.Message ?? "None");

        // Note: We intentionally do NOT remove the player session here.
        // This allows the player to reconnect and recover their state (RF-012).
        // Sessions can be cleaned up with a background task if needed.

        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Checks if a player has an existing session and can reconnect.
    /// Implements RF-012 (reconnection support).
    /// </summary>
    /// <param name="playerId">The player's unique ID.</param>
    /// <returns>The room code if the player has an active session, otherwise null.</returns>
    public Task<string?> CheckPlayerSession(Guid playerId)
    {
        lock (_sessionLock)
        {
            if (_playerSessions.TryGetValue(playerId, out var session))
            {
                _logger.LogInformation("Player {PlayerId} has existing session in room {RoomCode}.", 
                    playerId, session.RoomCode);
                return Task.FromResult<string?>(session.RoomCode);
            }
        }

        _logger.LogInformation("No existing session found for player {PlayerId}.", playerId);
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Removes a player's session. Used when explicitly leaving a room.
    /// </summary>
    /// <param name="playerId">The player's unique ID.</param>
    public Task LeaveRoom(Guid playerId)
    {
        lock (_sessionLock)
        {
            if (_playerSessions.Remove(playerId, out var session))
            {
                _logger.LogInformation("Player {PlayerId} left room {RoomCode}.", playerId, session.RoomCode);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Records player activity to update their status (RF-014).
    /// This method should be called periodically by connected clients.
    /// </summary>
    /// <param name="roomCode">The room code.</param>
    /// <param name="playerId">The player's unique ID.</param>
    public async Task RecordActivity(string roomCode, Guid playerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomCode, nameof(roomCode));

        if (!RoomCode.TryFrom(roomCode, out var code))
        {
            _logger.LogWarning("Invalid room code format: {RoomCode}.", roomCode);
            return;
        }

        var room = await _roomRepository.GetByCodeAsync(code);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomCode} not found when recording activity.", roomCode);
            return;
        }

        var player = room.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            _logger.LogWarning("Player {PlayerId} not found in room {RoomCode}.", playerId, roomCode);
            return;
        }

        // Update player activity
        player.RecordActivity();
        await _roomRepository.UpdateAsync(room);

        _logger.LogInformation("Activity recorded for player {PlayerId} in room {RoomCode}.", playerId, roomCode);

        // Broadcast updated room state (RF-016)
        await BroadcastRoomState(roomCode);
    }
}
