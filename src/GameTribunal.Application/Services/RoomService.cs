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

    public RoomService(IRoomRepository roomRepository, IRoomCodeGenerator roomCodeGenerator, ILogger<RoomService> logger)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _roomCodeGenerator = roomCodeGenerator ?? throw new ArgumentNullException(nameof(roomCodeGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RoomDto> CreateRoomAsync(GameMode mode, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting room creation for mode {GameMode}.", mode);

        var uniqueCode = await GenerateUniqueRoomCodeAsync(cancellationToken).ConfigureAwait(false);
        var room = Room.Create(uniqueCode, mode);

        _logger.LogInformation("Room {RoomCode} generated. Persisting room to repository.", room.Code.Value);
        await _roomRepository.AddAsync(room, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Room {RoomCode} persisted successfully for mode {GameMode}.", room.Code.Value, mode);

        return new RoomDto(room.Code.Value, room.Mode);
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
