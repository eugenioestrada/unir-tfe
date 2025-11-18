namespace GameTribunal.Domain.Enumerations;

/// <summary>
/// Represents the connection status of a player.
/// Implements RF-013: Player connection status tracking.
/// </summary>
public enum PlayerConnectionStatus
{
    /// <summary>
    /// Player is actively connected and interacting (green indicator).
    /// </summary>
    Conectado = 0,

    /// <summary>
    /// Player is connected but inactive for 30+ seconds (yellow indicator).
    /// Implements RF-014.
    /// </summary>
    Inactivo = 1,

    /// <summary>
    /// Player has been inactive for 5+ minutes or disconnected (gray indicator).
    /// Implements RF-015.
    /// </summary>
    Desconectado = 2
}
