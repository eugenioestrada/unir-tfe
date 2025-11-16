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
        _logger.LogInformation("Iniciando creación de sala para modo {GameMode}.", mode);

        var uniqueCode = await GenerateUniqueRoomCodeAsync(cancellationToken).ConfigureAwait(false);
        var room = Room.Create(uniqueCode, mode);

        _logger.LogInformation("Sala {RoomCode} generada. Persistiendo sala en el repositorio.", room.Code.Value);
        await _roomRepository.AddAsync(room, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Sala {RoomCode} persistida correctamente para modo {GameMode}.", room.Code.Value, mode);

        return new RoomDto(room.Code.Value, room.Mode);
    }

    private async Task<RoomCode> GenerateUniqueRoomCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            _logger.LogInformation("Generando código de sala. Intento {Attempt} de {MaxAttempts}.", attempt + 1, MaxGenerationAttempts);

            var candidate = _roomCodeGenerator.Generate();
            _logger.LogInformation("Código generado {RoomCode}. Verificando unicidad.", candidate.Value);
            var exists = await _roomRepository.ExistsAsync(candidate, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                _logger.LogInformation("Código de sala {RoomCode} aceptado en el intento {Attempt}.", candidate.Value, attempt + 1);
                return candidate;
            }

            _logger.LogWarning("Colisión detectada para el código {RoomCode} en el intento {Attempt}. Reintentando.", candidate.Value, attempt + 1);
        }

        _logger.LogError("No fue posible generar un código de sala único después de {MaxAttempts} intentos.", MaxGenerationAttempts);
        throw new InvalidOperationException("Unable to generate a unique room code after several attempts.");
    }
}
