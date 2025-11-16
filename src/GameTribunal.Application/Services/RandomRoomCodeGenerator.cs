using System.Security.Cryptography;
using GameTribunal.Application.Contracts;
using GameTribunal.Domain.ValueObjects;

namespace GameTribunal.Application.Services;

/// <summary>
/// Generates six-character alphanumeric codes avoiding visually confusing characters.
/// </summary>
public sealed class RandomRoomCodeGenerator : IRoomCodeGenerator
{
    private const int CodeLength = 6;
    private static readonly char[] AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public RoomCode Generate()
    {
        Span<char> buffer = stackalloc char[CodeLength];
        Span<byte> randomBytes = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(randomBytes);

        for (var i = 0; i < CodeLength; i++)
        {
            buffer[i] = AllowedCharacters[randomBytes[i] % AllowedCharacters.Length];
        }

        return RoomCode.From(new string(buffer));
    }
}
