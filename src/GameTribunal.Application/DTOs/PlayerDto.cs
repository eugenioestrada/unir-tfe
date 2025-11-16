namespace GameTribunal.Application.DTOs;

/// <summary>
/// Data transfer object for player information.
/// </summary>
/// <param name="Id">The unique identifier of the player.</param>
/// <param name="Alias">The player's display name.</param>
public sealed record PlayerDto(Guid Id, string Alias);
