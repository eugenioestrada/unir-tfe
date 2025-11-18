using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
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

    // RF-013 Tests: Player connection status tracking
    [Fact]
    public void Create_InitializesAsConectado()
    {
        var player = Player.Create("TestPlayer");

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
    }

    [Fact]
    public void RecordActivity_SetsStatusToConectado()
    {
        var player = Player.Create("TestPlayer");
        
        // Manually set to inactive
        player.UpdateConnectionStatus();
        
        // Record activity should reset to Conectado
        player.RecordActivity();

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
    }

    [Fact]
    public void RecordActivity_UpdatesLastActivityTime()
    {
        var player = Player.Create("TestPlayer");
        var initialTime = player.LastActivityAt;

        Thread.Sleep(100); // Small delay to ensure time difference
        player.RecordActivity();

        Assert.True(player.LastActivityAt > initialTime);
    }

    [Fact]
    public void MarkAsDisconnected_SetsStatusToDesconectado()
    {
        var player = Player.Create("TestPlayer");

        player.MarkAsDisconnected();

        Assert.Equal(PlayerConnectionStatus.Desconectado, player.ConnectionStatus);
    }

    // RF-014 Test: Inactive after 30 seconds
    [Fact]
    public void UpdateConnectionStatus_After30Seconds_SetsInactivo()
    {
        var player = Player.Create("TestPlayer");
        
        // Use reflection to set LastActivityAt to 31 seconds ago
        var lastActivityField = typeof(Player).GetProperty("LastActivityAt")!;
        lastActivityField.SetValue(player, DateTime.UtcNow.AddSeconds(-31));

        player.UpdateConnectionStatus();

        Assert.Equal(PlayerConnectionStatus.Inactivo, player.ConnectionStatus);
    }

    // RF-015 Test: Disconnected after 5 minutes
    [Fact]
    public void UpdateConnectionStatus_After5Minutes_SetsDesconectado()
    {
        var player = Player.Create("TestPlayer");
        
        // Use reflection to set LastActivityAt to 301 seconds ago (just over 5 minutes)
        var lastActivityField = typeof(Player).GetProperty("LastActivityAt")!;
        lastActivityField.SetValue(player, DateTime.UtcNow.AddSeconds(-301));

        player.UpdateConnectionStatus();

        Assert.Equal(PlayerConnectionStatus.Desconectado, player.ConnectionStatus);
    }

    [Fact]
    public void UpdateConnectionStatus_WithinActiveThreshold_RemainsConectado()
    {
        var player = Player.Create("TestPlayer");
        
        // Activity within last 30 seconds
        var lastActivityField = typeof(Player).GetProperty("LastActivityAt")!;
        lastActivityField.SetValue(player, DateTime.UtcNow.AddSeconds(-15));

        player.UpdateConnectionStatus();

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
    }
}
