using GameTribunal.Application.Contracts;
using GameTribunal.Application.DTOs;
using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GameTribunal.Application.Services;

/// <summary>
/// Handles room-related application use cases.
/// </summary>
public sealed class RoomService
{
    private const int MaxGenerationAttempts = 10;

    private readonly IRoomRepository _roomRepository;
    private readonly IRoomCodeGenerator _roomCodeGenerator;
    private readonly ILogger<RoomService> _logger;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public RoomService(
        IRoomRepository roomRepository,
        IRoomCodeGenerator roomCodeGenerator,
        ILogger<RoomService> logger,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(roomRepository);
        ArgumentNullException.ThrowIfNull(roomCodeGenerator);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(guidGenerator);
        ArgumentNullException.ThrowIfNull(clock);

        _roomRepository = roomRepository;
        _roomCodeGenerator = roomCodeGenerator;
        _logger = logger;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public async Task<RoomDto> CreateRoomAsync(GameMode mode, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting room creation for mode {GameMode}.", mode);

        var uniqueCode = await GenerateUniqueRoomCodeAsync(cancellationToken).ConfigureAwait(false);
        var room = Room.Create(uniqueCode, mode);

        _logger.LogInformation("Room {RoomCode} generated. Persisting room to repository.", room.Code.Value);
        await _roomRepository.AddAsync(room, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Room {RoomCode} persisted successfully for mode {GameMode}.", room.Code.Value, mode);

        return MapToDto(room);
    }

    /// <summary>
    /// Adds a player to a room with the specified alias.
    /// </summary>
    /// <param name="roomCode">The room code.</param>
    /// <param name="alias">The player's alias.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated room DTO with the new player.</returns>
    public async Task<RoomDto> JoinRoomAsync(string roomCode, string alias, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomCode, nameof(roomCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(alias, nameof(alias));

        if (!RoomCode.TryFrom(roomCode, out var code))
        {
            throw new ArgumentException("Invalid room code format.", nameof(roomCode));
        }

        _logger.LogInformation("Player attempting to join room {RoomCode} with alias {Alias}.", code.Value, alias);

        var room = await _roomRepository.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false);
        if (room is null)
        {
            _logger.LogWarning("Room {RoomCode} not found.", code.Value);
            throw new InvalidOperationException($"Room with code {code.Value} does not exist.");
        }

        var playerId = _guidGenerator.Create();
        var joinedAt = _clock.UtcNow;
        var player = room.AddPlayer(alias, playerId, joinedAt);
        _logger.LogInformation(
            "Player {PlayerId} with alias {Alias} added to room {RoomCode} at {JoinedAtUtc}.",
            player.Id,
            player.Alias,
            room.Code.Value,
            joinedAt);

        await _roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Room {RoomCode} updated with new player.", room.Code.Value);

        return MapToDto(room);
    }

    /// <summary>
    /// Generates the QR code URL for a room.
    /// </summary>
    /// <param name="roomCode">The room code.</param>
    /// <param name="baseUrl">The base URL of the application (e.g., "https://app.example.com").</param>
    /// <returns>The full URL that can be encoded in a QR code.</returns>
    public string GenerateRoomUrl(string roomCode, string baseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomCode, nameof(roomCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl, nameof(baseUrl));

        if (!RoomCode.TryFrom(roomCode, out _))
        {
            throw new ArgumentException("Invalid room code format.", nameof(roomCode));
        }

        var trimmedBaseUrl = baseUrl.TrimEnd('/');
        return $"{trimmedBaseUrl}/join/{roomCode}";
    }

    /// <summary>
    /// Gets a room by its code. Returns null if not found (RF-017).
    /// </summary>
    public async Task<RoomDto?> GetRoomByCodeAsync(RoomCode roomCode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roomCode);

        var room = await _roomRepository.GetByCodeAsync(roomCode, cancellationToken).ConfigureAwait(false);
        return room is null ? null : MapToDto(room);
    }

    private static RoomDto MapToDto(Room room)
    {
        var players = room.Players.Select(p => new PlayerDto(p.Id, p.Alias, p.ConnectionStatus)).ToList();
        return new RoomDto(room.Code.Value, room.Mode, players, room.CanStartGame());
    }

    private async Task<RoomCode> GenerateUniqueRoomCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            _logger.LogInformation("Generating room code. Attempt {Attempt} of {MaxAttempts}.", attempt + 1, MaxGenerationAttempts);

            var candidate = _roomCodeGenerator.Generate();
            _logger.LogInformation("Generated code {RoomCode}. Verifying uniqueness.", candidate.Value);
            var exists = await _roomRepository.ExistsAsync(candidate, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                _logger.LogInformation("Room code {RoomCode} accepted on attempt {Attempt}.", candidate.Value, attempt + 1);
                return candidate;
            }

            _logger.LogWarning("Collision detected for code {RoomCode} on attempt {Attempt}. Retrying.", candidate.Value, attempt + 1);
        }

        _logger.LogError("Unable to generate a unique room code after {MaxAttempts} attempts.", MaxGenerationAttempts);
        throw new InvalidOperationException("Unable to generate a unique room code after several attempts.");
    }
}
