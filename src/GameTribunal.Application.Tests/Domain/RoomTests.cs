using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;
using Xunit;

namespace GameTribunal.Application.Tests.Domain;

public sealed class RoomTests
{
    [Fact]
    public void Create_ValidParameters_CreatesRoom()
    {
        var code = RoomCode.From("ABC123");
        var room = Room.Create(code, GameMode.Normal);

        Assert.NotNull(room);
        Assert.Equal(code, room.Code);
        Assert.Equal(GameMode.Normal, room.Mode);
        Assert.Empty(room.Players);
    }

    [Fact]
    public void Create_NullCode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Room.Create(null!, GameMode.Normal));
    }

    [Fact]
    public void AddPlayer_ValidAlias_AddsPlayer()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        var player = room.AddPlayer("Player1");

        Assert.NotNull(player);
        Assert.Equal("Player1", player.Alias);
        Assert.Single(room.Players);
        Assert.Contains(player, room.Players);
    }

    [Fact]
    public void AddPlayer_MultiplePlayers_AddsAllPlayers()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        room.AddPlayer("Player1");
        room.AddPlayer("Player2");
        room.AddPlayer("Player3");

        Assert.Equal(3, room.Players.Count);
    }

    [Fact]
    public void AddPlayer_DuplicateAlias_Throws()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        room.AddPlayer("Player1");

        Assert.Throws<InvalidOperationException>(() => room.AddPlayer("Player1"));
    }

    [Fact]
    public void AddPlayer_DuplicateAliasCaseInsensitive_Throws()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        room.AddPlayer("Player1");

        Assert.Throws<InvalidOperationException>(() => room.AddPlayer("PLAYER1"));
        Assert.Throws<InvalidOperationException>(() => room.AddPlayer("player1"));
    }

    [Fact]
    public void AddPlayer_MaxPlayers_ThrowsOnExceedingLimit()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        // Add 16 players (max capacity)
        for (int i = 1; i <= Room.MaxPlayers; i++)
        {
            room.AddPlayer($"Player{i}");
        }

        Assert.Equal(Room.MaxPlayers, room.Players.Count);
        Assert.Throws<InvalidOperationException>(() => room.AddPlayer("ExtraPlayer"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPlayer_WhitespaceAlias_Throws(string? alias)
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);

        Assert.Throws<ArgumentException>(() => room.AddPlayer(alias!));
    }

    [Fact]
    public void AddPlayer_NullAlias_Throws()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);

        Assert.Throws<ArgumentNullException>(() => room.AddPlayer(null!));
    }

    [Fact]
    public void CanStartGame_LessThan4Players_ReturnsFalse()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        Assert.False(room.CanStartGame());

        room.AddPlayer("Player1");
        Assert.False(room.CanStartGame());

        room.AddPlayer("Player2");
        Assert.False(room.CanStartGame());

        room.AddPlayer("Player3");
        Assert.False(room.CanStartGame());
    }

    [Fact]
    public void CanStartGame_Exactly4Players_ReturnsTrue()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        room.AddPlayer("Player1");
        room.AddPlayer("Player2");
        room.AddPlayer("Player3");
        room.AddPlayer("Player4");

        Assert.True(room.CanStartGame());
    }

    [Fact]
    public void CanStartGame_MoreThan4Players_ReturnsTrue()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        for (int i = 1; i <= 8; i++)
        {
            room.AddPlayer($"Player{i}");
        }

        Assert.True(room.CanStartGame());
    }

    [Fact]
    public void CanStartGame_MaxPlayers_ReturnsTrue()
    {
        var room = Room.Create(RoomCode.From("ABC123"), GameMode.Normal);
        
        for (int i = 1; i <= Room.MaxPlayers; i++)
        {
            room.AddPlayer($"Player{i}");
        }

        Assert.True(room.CanStartGame());
    }

    [Theory]
    [InlineData(GameMode.Suave)]
    [InlineData(GameMode.Normal)]
    [InlineData(GameMode.Spicy)]
    public void Create_AllGameModes_Succeeds(GameMode mode)
    {
        var room = Room.Create(RoomCode.From("ABC123"), mode);

        Assert.Equal(mode, room.Mode);
    }
}
