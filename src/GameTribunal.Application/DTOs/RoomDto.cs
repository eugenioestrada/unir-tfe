using GameTribunal.Domain.Enumerations;

namespace GameTribunal.Application.DTOs;

/// <summary>
/// Data transfer object representing the state of a room exposed to clients.
/// </summary>
public sealed record RoomDto(string Code, GameMode Mode);
