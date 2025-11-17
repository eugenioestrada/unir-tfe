using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Domain.Entities;

/// <summary>
/// Aggregate root representing a game room.
/// </summary>
public sealed class Room
{
    public const int MinPlayers = 4;
    public const int MaxPlayers = 16;

    private readonly List<Player> _players = new();

    private Room(RoomCode code, GameMode mode)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Mode = mode;
    }

    public RoomCode Code { get; }

    public GameMode Mode { get; }

    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();

    public static Room Create(RoomCode code, GameMode mode) => new(code, mode);

    /// <summary>
    /// Adds a player to the room with the specified alias.
    /// </summary>
    /// <param name="alias">The player's alias.</param>
    /// <returns>The created Player.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the room is full or alias is duplicated.</exception>
    public Player AddPlayer(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias, nameof(alias));

        if (_players.Count >= MaxPlayers)
        {
            throw new InvalidOperationException($"Cannot add player. Room has reached the maximum capacity of {MaxPlayers} players.");
        }

        var trimmedAlias = alias.Trim();
        if (_players.Any(p => string.Equals(p.Alias, trimmedAlias, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A player with alias '{trimmedAlias}' already exists in this room.");
        }

        var player = Player.Create(trimmedAlias);
        _players.Add(player);
        return player;
    }

    /// <summary>
    /// Determines whether the game can be started based on the current number of players.
    /// </summary>
    /// <returns>True if there are at least MinPlayers players; otherwise, false.</returns>
    public bool CanStartGame()
    {
        return _players.Count >= MinPlayers;
    }
}
