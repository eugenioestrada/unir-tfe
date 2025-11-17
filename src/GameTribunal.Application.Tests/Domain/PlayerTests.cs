using GameTribunal.Domain.Entities;
using Xunit;

namespace GameTribunal.Application.Tests.Domain;

public sealed class PlayerTests
{
    [Fact]
    public void Create_ValidAlias_CreatesPlayer()
    {
        var player = Player.Create("ValidName");

        Assert.NotNull(player);
        Assert.Equal("ValidName", player.Alias);
        Assert.NotEqual(Guid.Empty, player.Id);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var player = Player.Create("  ValidName  ");

        Assert.Equal("ValidName", player.Alias);
    }

    [Theory]
    [InlineData("AB")] // 2 characters (minimum)
    [InlineData("ValidPlayer")]
    [InlineData("12345678901234567890")] // 20 characters (maximum)
    public void Create_ValidAliasLength_Succeeds(string alias)
    {
        var player = Player.Create(alias);

        Assert.Equal(alias, player.Alias);
    }

    [Theory]
    [InlineData("A")] // 1 character (too short)
    [InlineData("123456789012345678901")] // 21 characters (too long)
    public void Create_InvalidAliasLength_Throws(string alias)
    {
        Assert.Throws<ArgumentException>(() => Player.Create(alias));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhitespaceAlias_Throws(string? alias)
    {
        Assert.Throws<ArgumentException>(() => Player.Create(alias!));
    }

    [Fact]
    public void Create_NullAlias_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Player.Create(null!));
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var player1 = Player.Create("Player1");
        var player2 = Player.Create("Player2");

        Assert.NotEqual(player1.Id, player2.Id);
    }
}
