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
    }

    public Guid Id { get; }

    public string Alias { get; }

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
}
