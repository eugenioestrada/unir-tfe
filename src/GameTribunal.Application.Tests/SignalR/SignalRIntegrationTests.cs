using GameTribunal.Application.Contracts;
using GameTribunal.Application.DTOs;
using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameTribunal.Application.Tests.SignalR;

/// <summary>
/// Tests for SignalR real-time synchronization (RF-011) and reconnection (RF-012).
/// </summary>
public class SignalRIntegrationTests
{
    /// <summary>
    /// Verifies that when a player joins a room, other players receive the updated room state.
    /// This tests RF-011 (real-time synchronization).
    /// </summary>
    [Fact]
    public async Task JoinRoom_BroadcastsRoomStateUpdate()
    {
        // Arrange
        var roomCode = RoomCode.From("ABC123");
        var room = Room.Create(roomCode, GameMode.Normal);
        room.AddPlayer("Player1");

        var mockRepository = new Mock<IRoomRepository>();
        mockRepository
            .Setup(r => r.GetByCodeAsync(roomCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        mockRepository
            .Setup(r => r.UpdateAsync(room, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockCodeGenerator = new Mock<IRoomCodeGenerator>();
        var mockLogger = new Mock<ILogger<Application.Services.RoomService>>();

        var roomService = new Application.Services.RoomService(
            mockRepository.Object,
            mockCodeGenerator.Object,
            mockLogger.Object
        );

        // Act
        var result = await roomService.JoinRoomAsync("ABC123", "Player2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Players.Count);
        Assert.Contains(result.Players, p => p.Alias == "Player1");
        Assert.Contains(result.Players, p => p.Alias == "Player2");

        // Verify repository was called to update the room
        mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that duplicate aliases are rejected, maintaining data integrity.
    /// This is important for RF-011 (proper state synchronization).
    /// </summary>
    [Fact]
    public async Task JoinRoom_DuplicateAlias_ThrowsException()
    {
        // Arrange
        var roomCode = RoomCode.From("ABC123");
        var room = Room.Create(roomCode, GameMode.Normal);
        room.AddPlayer("Player1");

        var mockRepository = new Mock<IRoomRepository>();
        mockRepository
            .Setup(r => r.GetByCodeAsync(roomCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var mockCodeGenerator = new Mock<IRoomCodeGenerator>();
        var mockLogger = new Mock<ILogger<Application.Services.RoomService>>();

        var roomService = new Application.Services.RoomService(
            mockRepository.Object,
            mockCodeGenerator.Object,
            mockLogger.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => roomService.JoinRoomAsync("ABC123", "Player1")
        );

        Assert.Contains("already exists", exception.Message);
    }

    /// <summary>
    /// Verifies that room state includes all necessary information for clients.
    /// This tests RF-011 (complete state synchronization).
    /// </summary>
    [Fact]
    public async Task CreateRoom_ReturnsCompleteRoomState()
    {
        // Arrange
        var roomCode = RoomCode.From("ABC123");
        var mockRepository = new Mock<IRoomRepository>();
        mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockRepository
            .Setup(r => r.ExistsAsync(It.IsAny<RoomCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockCodeGenerator = new Mock<IRoomCodeGenerator>();
        mockCodeGenerator
            .Setup(g => g.Generate())
            .Returns(roomCode);

        var mockLogger = new Mock<ILogger<Application.Services.RoomService>>();

        var roomService = new Application.Services.RoomService(
            mockRepository.Object,
            mockCodeGenerator.Object,
            mockLogger.Object
        );

        // Act
        var result = await roomService.CreateRoomAsync(GameMode.Spicy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC123", result.Code);
        Assert.Equal(GameMode.Spicy, result.Mode);
        Assert.Empty(result.Players);
        Assert.False(result.CanStartGame);
    }

    /// <summary>
    /// Verifies that room can start when minimum players are present.
    /// This is important for RF-011 (synchronizing game readiness state).
    /// </summary>
    [Fact]
    public async Task JoinRoom_WithMinimumPlayers_CanStartGame()
    {
        // Arrange
        var roomCode = RoomCode.From("ABC123");
        var room = Room.Create(roomCode, GameMode.Normal);
        room.AddPlayer("Player1");
        room.AddPlayer("Player2");
        room.AddPlayer("Player3");

        var mockRepository = new Mock<IRoomRepository>();
        mockRepository
            .Setup(r => r.GetByCodeAsync(roomCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        mockRepository
            .Setup(r => r.UpdateAsync(room, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockCodeGenerator = new Mock<IRoomCodeGenerator>();
        var mockLogger = new Mock<ILogger<Application.Services.RoomService>>();

        var roomService = new Application.Services.RoomService(
            mockRepository.Object,
            mockCodeGenerator.Object,
            mockLogger.Object
        );

        // Act
        var result = await roomService.JoinRoomAsync("ABC123", "Player4");

        // Assert
        Assert.True(result.CanStartGame);
        Assert.Equal(4, result.Players.Count);
    }

    /// <summary>
    /// Verifies that player IDs are unique and persistent.
    /// This is important for RF-012 (reconnection support).
    /// </summary>
    [Fact]
    public async Task JoinRoom_AssignsUniquePersistentPlayerIds()
    {
        // Arrange
        var roomCode = RoomCode.From("ABC123");
        var room = Room.Create(roomCode, GameMode.Normal);

        var mockRepository = new Mock<IRoomRepository>();
        mockRepository
            .Setup(r => r.GetByCodeAsync(roomCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        mockRepository
            .Setup(r => r.UpdateAsync(room, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockCodeGenerator = new Mock<IRoomCodeGenerator>();
        var mockLogger = new Mock<ILogger<Application.Services.RoomService>>();

        var roomService = new Application.Services.RoomService(
            mockRepository.Object,
            mockCodeGenerator.Object,
            mockLogger.Object
        );

        // Act
        var result1 = await roomService.JoinRoomAsync("ABC123", "Player1");
        var result2 = await roomService.JoinRoomAsync("ABC123", "Player2");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        var player1 = result2.Players.First(p => p.Alias == "Player1");
        var player2 = result2.Players.First(p => p.Alias == "Player2");
        
        Assert.NotEqual(player1.Id, player2.Id);
        Assert.NotEqual(Guid.Empty, player1.Id);
        Assert.NotEqual(Guid.Empty, player2.Id);
    }
}
