using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using Xunit;

namespace GameTribunal.Application.Tests.Domain;

public sealed class PlayerTests
{
    [Fact]
    public void Create_ValidAlias_CreatesPlayer()
    {
        var identifier = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var player = Player.Create("ValidName", identifier, createdAt);

        Assert.Equal("ValidName", player.Alias);
        Assert.Equal(identifier, player.Id);
        Assert.Equal(createdAt, player.LastActivityAt);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var identifier = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var player = Player.Create("  ValidName  ", identifier, createdAt);

        Assert.Equal("ValidName", player.Alias);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("ValidPlayer")]
    [InlineData("12345678901234567890")]
    public void Create_ValidAliasLength_Succeeds(string alias)
    {
        var player = Player.Create(alias, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(alias, player.Alias);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("123456789012345678901")]
    public void Create_InvalidAliasLength_Throws(string alias)
    {
        Assert.Throws<ArgumentException>(() => Player.Create(alias, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhitespaceAlias_Throws(string? alias)
    {
        Assert.Throws<ArgumentException>(() => Player.Create(alias!, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Create_NullAlias_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Player.Create(null!, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Create_EmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() => Player.Create("Player", Guid.Empty, DateTime.UtcNow));
    }

    [Fact]
    public void Create_NonUtcTimestamp_Throws()
    {
        var nonUtc = DateTime.Now;

        Assert.Throws<ArgumentException>(() => Player.Create("Player", Guid.NewGuid(), nonUtc));
    }

    [Fact]
    public void Create_InitializesAsConectado()
    {
        var player = Player.Create("TestPlayer", Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
    }

    [Fact]
    public void RecordActivity_SetsStatusToConectado()
    {
        var initialActivity = DateTime.UtcNow.AddSeconds(-40);
        var player = Player.Create("TestPlayer", Guid.NewGuid(), initialActivity);

        var evaluationTime = initialActivity.AddSeconds(35);
        player.UpdateConnectionStatus(evaluationTime);

        var activityTime = evaluationTime.AddSeconds(1);
        player.RecordActivity(activityTime);

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
        Assert.Equal(activityTime, player.LastActivityAt);
    }

    [Fact]
    public void RecordActivity_UpdatesLastActivityTime()
    {
        var initialActivity = DateTime.UtcNow.AddSeconds(-10);
        var player = Player.Create("TestPlayer", Guid.NewGuid(), initialActivity);

        var newActivity = initialActivity.AddSeconds(5);
        player.RecordActivity(newActivity);

        Assert.Equal(newActivity, player.LastActivityAt);
    }

    [Fact]
    public void MarkAsDisconnected_SetsStatusToDesconectado()
    {
        var player = Player.Create("TestPlayer", Guid.NewGuid(), DateTime.UtcNow);

        player.MarkAsDisconnected();

        Assert.Equal(PlayerConnectionStatus.Desconectado, player.ConnectionStatus);
    }

    [Fact]
    public void UpdateConnectionStatus_After30Seconds_SetsInactivo()
    {
        var lastActivity = DateTime.UtcNow.AddSeconds(-31);
        var player = Player.Create("TestPlayer", Guid.NewGuid(), lastActivity);

        var evaluationTime = lastActivity.AddSeconds(31);
        player.UpdateConnectionStatus(evaluationTime);

        Assert.Equal(PlayerConnectionStatus.Inactivo, player.ConnectionStatus);
    }

    [Fact]
    public void UpdateConnectionStatus_After5Minutes_SetsDesconectado()
    {
        var lastActivity = DateTime.UtcNow.AddSeconds(-301);
        var player = Player.Create("TestPlayer", Guid.NewGuid(), lastActivity);

        var evaluationTime = lastActivity.AddSeconds(301);
        player.UpdateConnectionStatus(evaluationTime);

        Assert.Equal(PlayerConnectionStatus.Desconectado, player.ConnectionStatus);
    }

    [Fact]
    public void UpdateConnectionStatus_WithinActiveThreshold_RemainsConectado()
    {
        var lastActivity = DateTime.UtcNow.AddSeconds(-15);
        var player = Player.Create("TestPlayer", Guid.NewGuid(), lastActivity);

        var evaluationTime = lastActivity.AddSeconds(15);
        player.UpdateConnectionStatus(evaluationTime);

        Assert.Equal(PlayerConnectionStatus.Conectado, player.ConnectionStatus);
    }
}
