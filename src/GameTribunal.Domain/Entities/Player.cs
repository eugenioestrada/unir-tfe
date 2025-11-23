using GameTribunal.Domain.Enumerations;

namespace GameTribunal.Domain.Entities;

/// <summary>
/// Represents a player participating in a game room.
/// </summary>
public sealed class Player
{
    private const int MinAliasLength = 2;
    private const int MaxAliasLength = 20;

    private Player(Guid id, string alias, DateTime lastActivityAtUtc)
    {
        Id = id;
        Alias = alias;
        LastActivityAt = lastActivityAtUtc;
        ConnectionStatus = PlayerConnectionStatus.Conectado;
    }

    public Guid Id { get; }

    public string Alias { get; }

    /// <summary>
    /// Current connection status of the player (RF-013).
    /// </summary>
    public PlayerConnectionStatus ConnectionStatus { get; private set; }

    /// <summary>
    /// Timestamp of the last player activity (RF-014, RF-015).
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    public static Player Create(string alias, Guid id, DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias, nameof(alias));
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Player identifier cannot be empty.", nameof(id));
        }

        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        var trimmedAlias = alias.Trim();
        if (trimmedAlias.Length < MinAliasLength)
        {
            throw new ArgumentException($"Alias must be at least {MinAliasLength} characters long.", nameof(alias));
        }

        if (trimmedAlias.Length > MaxAliasLength)
        {
            throw new ArgumentException($"Alias cannot exceed {MaxAliasLength} characters.", nameof(alias));
        }

        return new Player(id, trimmedAlias, createdAtUtc);
    }

    /// <summary>
    /// Updates the last activity timestamp and sets status to Conectado (RF-014).
    /// </summary>
    public void RecordActivity(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        LastActivityAt = utcNow;
        ConnectionStatus = PlayerConnectionStatus.Conectado;
    }

    /// <summary>
    /// Updates the connection status based on elapsed time since last activity (RF-014, RF-015).
    /// </summary>
    public void UpdateConnectionStatus(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));

        var timeSinceLastActivity = utcNow - LastActivityAt;

        // RF-015: 5 minutes (300 seconds) -> Desconectado
        if (timeSinceLastActivity.TotalSeconds >= 300)
        {
            ConnectionStatus = PlayerConnectionStatus.Desconectado;
        }
        // RF-014: 30 seconds -> Inactivo
        else if (timeSinceLastActivity.TotalSeconds >= 30)
        {
            ConnectionStatus = PlayerConnectionStatus.Inactivo;
        }
        // Active
        else
        {
            ConnectionStatus = PlayerConnectionStatus.Conectado;
        }
    }

    /// <summary>
    /// Sets the status to Desconectado when a player explicitly disconnects (RF-015).
    /// </summary>
    public void MarkAsDisconnected()
    {
        ConnectionStatus = PlayerConnectionStatus.Desconectado;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be expressed in UTC", parameterName);
        }
    }
}
