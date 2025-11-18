using System.Collections.Concurrent;
using GameTribunal.Application.Contracts;
using GameTribunal.Domain.Entities;
using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Infrastructure.Persistence;

/// <summary>
/// Thread-safe in-memory repository suited for early development and testing scenarios.
/// </summary>
public sealed class InMemoryRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<RoomCode, Room> _rooms = new();

    public Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        return Task.FromResult(_rooms.ContainsKey(code));
    }

    public Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!_rooms.TryAdd(room.Code, room))
        {
            throw new InvalidOperationException($"A room with code {room.Code} already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        _rooms.TryGetValue(code, out var room);
        return Task.FromResult(room);
    }

    public Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);
        _rooms[room.Code] = room;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Room>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rooms = _rooms.Values.ToList();
        return Task.FromResult<IReadOnlyCollection<Room>>(rooms);
    }
}
