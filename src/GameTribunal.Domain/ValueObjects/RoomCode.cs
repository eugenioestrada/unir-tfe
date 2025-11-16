using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GameTribunal.Domain.ValueObjects;

/// <summary>
/// Immutable identifier used to reference a room. Enforces formatting rules and value semantics.
/// </summary>
public sealed class RoomCode : IEquatable<RoomCode>
{
    private static readonly Regex CodePattern = new("^[A-Z0-9]{6}$", RegexOptions.Compiled);

    private RoomCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RoomCode From(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (!CodePattern.IsMatch(value))
        {
            throw new ArgumentException("Room codes must be exactly six uppercase alphanumeric characters.", nameof(value));
        }

        return new RoomCode(value);
    }

    public override string ToString() => Value;

    public bool Equals(RoomCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is RoomCode other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(RoomCode? left, RoomCode? right) => Equals(left, right);

    public static bool operator !=(RoomCode? left, RoomCode? right) => !Equals(left, right);

    public static bool TryFrom(string value, [NotNullWhen(true)] out RoomCode? code)
    {
        code = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!CodePattern.IsMatch(value))
        {
            return false;
        }

        code = new RoomCode(value);
        return true;
    }
}
