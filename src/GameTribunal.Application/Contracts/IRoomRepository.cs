using GameTribunal.Domain.Entities;
using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Application.Contracts;

/// <summary>
/// Persistence abstraction for managing rooms.
/// </summary>
public interface IRoomRepository
{
    Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken = default);

    Task AddAsync(Room room, CancellationToken cancellationToken = default);

    Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken = default);

    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rooms. Used by background services to monitor player statuses (RF-013).
    /// </summary>
    Task<IReadOnlyCollection<Room>> GetAllAsync(CancellationToken cancellationToken = default);
}
