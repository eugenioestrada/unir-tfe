using GameTribunal.Domain.Enumerations;

namespace GameTribunal.Domain.Entities;

/// <summary>
/// Represents a player participating in a game room.
/// </summary>
public sealed class Player
{
    private const int MinAliasLength = 2;
    private const int MaxAliasLength = 20;

    private Player(string alias)
    {
        Alias = alias;
        Id = Guid.NewGuid();
        ConnectionStatus = PlayerConnectionStatus.Conectado;
        LastActivityAt = DateTime.UtcNow;
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

    public static Player Create(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias, nameof(alias));
        
        var trimmedAlias = alias.Trim();
        if (trimmedAlias.Length < MinAliasLength)
        {
            throw new ArgumentException($"Alias must be at least {MinAliasLength} characters long.", nameof(alias));
        }

        if (trimmedAlias.Length > MaxAliasLength)
        {
            throw new ArgumentException($"Alias cannot exceed {MaxAliasLength} characters.", nameof(alias));
        }

        return new Player(trimmedAlias);
    }

    /// <summary>
    /// Updates the last activity timestamp and sets status to Conectado (RF-014).
    /// </summary>
    public void RecordActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        ConnectionStatus = PlayerConnectionStatus.Conectado;
    }

    /// <summary>
    /// Updates the connection status based on elapsed time since last activity (RF-014, RF-015).
    /// </summary>
    public void UpdateConnectionStatus()
    {
        var timeSinceLastActivity = DateTime.UtcNow - LastActivityAt;

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
}
