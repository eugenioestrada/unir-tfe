using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Application.Contracts;

/// <summary>
/// Generates room codes following the project's rules.
/// </summary>
public interface IRoomCodeGenerator
{
    RoomCode Generate();
}
