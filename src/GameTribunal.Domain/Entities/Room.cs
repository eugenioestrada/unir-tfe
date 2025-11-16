using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Domain.Entities;

/// <summary>
/// Aggregate root representing a game room.
/// </summary>
public sealed class Room
{
    private Room(RoomCode code, GameMode mode)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Mode = mode;
    }

    public RoomCode Code { get; }

    public GameMode Mode { get; }

    public static Room Create(RoomCode code, GameMode mode) => new(code, mode);
}
