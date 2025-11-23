using System;
using System.Collections.Generic;
using System.Linq;
using GameTribunal.Application.Contracts;
using GameTribunal.Application.Services;
using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameTribunal.Application.Tests.Services;

public sealed class RoomServiceTests
{
    [Fact]
    public async Task CreateRoomAsync_GeneratesUniqueCodeAndPersistsRoom()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        var result = await service.CreateRoomAsync(GameMode.Suave);

        Assert.Equal(code.Value, result.Code);
        Assert.Equal(GameMode.Suave, result.Mode);
        Assert.Empty(result.Players);
        Assert.False(result.CanStartGame);
        Assert.Single(repository.StoredRooms);
        Assert.Equal(code, repository.StoredRooms[0].Code);
    }

    [Fact]
    public async Task CreateRoomAsync_RetriesWhenGeneratedCodeAlreadyExists()
    {
        var existingCode = RoomCode.From("ABC123");
        var freshCode = RoomCode.From("XYZ789");
        var repository = new InMemoryRoomRepository(existingCode);
        var generator = new StubRoomCodeGenerator(existingCode, freshCode);
        var service = CreateRoomService(repository, generator);

        var result = await service.CreateRoomAsync(GameMode.Normal);

        Assert.Equal(freshCode.Value, result.Code);
        Assert.Empty(result.Players);
        Assert.False(result.CanStartGame);
        Assert.Contains(repository.StoredRooms, room => room.Code == freshCode);
        Assert.True(generator.GeneratedCodes.Count == 2, "RoomService should have requested a second code on collision.");
    }

    [Fact]
    public async Task CreateRoomAsync_ThrowsWhenNoUniqueCodeAvailable()
    {
        var conflictingCode = RoomCode.From("ABC123");
        var repository = new AlwaysConflictingRoomRepository();
        var generator = new StubRoomCodeGenerator(Enumerable.Repeat(conflictingCode, 20).ToArray());
        var service = CreateRoomService(repository, generator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRoomAsync(GameMode.Spicy));
        Assert.Empty(repository.StoredRooms);
    }

    // RF-005: Lobby phase where players join
    [Fact]
    public async Task JoinRoomAsync_AddsPlayerToRoom()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        var result = await service.JoinRoomAsync("ABC123", "PlayerOne");

        Assert.Equal("ABC123", result.Code);
        Assert.Single(result.Players);
        Assert.Equal("PlayerOne", result.Players.First().Alias);
        Assert.False(result.CanStartGame); // Only 1 player, need 4 minimum
    }

    // RF-006: Prevent duplicate aliases
    [Fact]
    public async Task JoinRoomAsync_ThrowsWhenAliasIsDuplicated()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        await service.JoinRoomAsync("ABC123", "PlayerOne");

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.JoinRoomAsync("ABC123", "PlayerOne"));
    }

    // RF-006: Prevent duplicate aliases (case-insensitive)
    [Fact]
    public async Task JoinRoomAsync_ThrowsWhenAliasIsDuplicatedCaseInsensitive()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        await service.JoinRoomAsync("ABC123", "PlayerOne");

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.JoinRoomAsync("ABC123", "playerone"));
    }

    // RF-003: Room must support 4-16 players
    [Fact]
    public async Task JoinRoomAsync_ThrowsWhenRoomIsFull()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        
        // Add 16 players (max capacity)
        for (int i = 1; i <= 16; i++)
        {
            await service.JoinRoomAsync("ABC123", $"Player{i}");
        }

        // Try to add 17th player
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.JoinRoomAsync("ABC123", "Player17"));
    }

    // RF-003: Cannot start with less than 4 players
    [Fact]
    public async Task JoinRoomAsync_CannotStartGameWithLessThan4Players()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        
        var result1 = await service.JoinRoomAsync("ABC123", "Player1");
        Assert.False(result1.CanStartGame);

        var result2 = await service.JoinRoomAsync("ABC123", "Player2");
        Assert.False(result2.CanStartGame);

        var result3 = await service.JoinRoomAsync("ABC123", "Player3");
        Assert.False(result3.CanStartGame);
    }

    // RF-003: Can start with 4 or more players
    [Fact]
    public async Task JoinRoomAsync_CanStartGameWith4OrMorePlayers()
    {
        var code = RoomCode.From("ABC123");
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(code);
        var service = CreateRoomService(repository, generator);

        await service.CreateRoomAsync(GameMode.Normal);
        
        await service.JoinRoomAsync("ABC123", "Player1");
        await service.JoinRoomAsync("ABC123", "Player2");
        await service.JoinRoomAsync("ABC123", "Player3");
        var result = await service.JoinRoomAsync("ABC123", "Player4");

        Assert.True(result.CanStartGame);
        Assert.Equal(4, result.Players.Count);
    }

    [Fact]
    public async Task JoinRoomAsync_ThrowsWhenRoomDoesNotExist()
    {
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(RoomCode.From("ABC123"));
        var service = CreateRoomService(repository, generator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.JoinRoomAsync("XYZ999", "PlayerOne"));
    }

    [Fact]
    public async Task JoinRoomAsync_ThrowsWhenRoomCodeIsInvalid()
    {
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(RoomCode.From("ABC123"));
        var service = CreateRoomService(repository, generator);

        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.JoinRoomAsync("INVALID", "PlayerOne"));
    }

    // RF-004: QR code URL generation
    [Fact]
    public void GenerateRoomUrl_CreatesCorrectUrl()
    {
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(RoomCode.From("ABC123"));
        var service = CreateRoomService(repository, generator);

        var url = service.GenerateRoomUrl("ABC123", "https://example.com");

        Assert.Equal("https://example.com/join/ABC123", url);
    }

    [Fact]
    public void GenerateRoomUrl_HandlesTrailingSlashInBaseUrl()
    {
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(RoomCode.From("ABC123"));
        var service = CreateRoomService(repository, generator);

        var url = service.GenerateRoomUrl("ABC123", "https://example.com/");

        Assert.Equal("https://example.com/join/ABC123", url);
    }

    [Fact]
    public void GenerateRoomUrl_ThrowsWhenRoomCodeIsInvalid()
    {
        var repository = new InMemoryRoomRepository();
        var generator = new StubRoomCodeGenerator(RoomCode.From("ABC123"));
        var service = CreateRoomService(repository, generator);

        Assert.Throws<ArgumentException>(() => 
            service.GenerateRoomUrl("INVALID", "https://example.com"));
    }

    private static RoomService CreateRoomService(
        IRoomRepository repository,
        IRoomCodeGenerator codeGenerator,
        TestGuidGenerator? guidGenerator = null,
        TestClock? clock = null)
    {
        var resolvedGuidGenerator = guidGenerator ?? new TestGuidGenerator();
        var resolvedClock = clock ?? new TestClock();
        return new RoomService(repository, codeGenerator, NullLogger<RoomService>.Instance, resolvedGuidGenerator, resolvedClock);
    }

    private sealed class StubRoomCodeGenerator : IRoomCodeGenerator
    {
        private readonly Queue<RoomCode> _codes;

        public StubRoomCodeGenerator(params RoomCode[] codes)
        {
            if (codes is null || codes.Length == 0)
            {
                throw new ArgumentException("At least one code must be provided for the stub generator.", nameof(codes));
            }

            _codes = new Queue<RoomCode>(codes);
        }

        public IReadOnlyCollection<RoomCode> GeneratedCodes => _generatedCodes;

        private readonly List<RoomCode> _generatedCodes = new();

        public RoomCode Generate()
        {
            var code = _codes.Count > 1 ? _codes.Dequeue() : _codes.Peek();
            _generatedCodes.Add(code);
            return code;
        }
    }

    private sealed class InMemoryRoomRepository : IRoomRepository
    {
        private readonly HashSet<RoomCode> _existingCodes = new();

        public InMemoryRoomRepository(params RoomCode[] existingCodes)
        {
            foreach (var code in existingCodes)
            {
                _existingCodes.Add(code);
            }
        }

        public List<Room> StoredRooms { get; } = new();

        public Task AddAsync(Room room, CancellationToken cancellationToken = default)
        {
            StoredRooms.Add(room);
            _existingCodes.Add(room.Code);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_existingCodes.Contains(code));
        }

        public Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken = default)
        {
            var room = StoredRooms.FirstOrDefault(r => r.Code == code);
            return Task.FromResult(room);
        }

        public Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Room>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Room>>(StoredRooms);
        }
    }

    private sealed class AlwaysConflictingRoomRepository : IRoomRepository
    {
        public List<Room> StoredRooms { get; } = new();

        public Task AddAsync(Room room, CancellationToken cancellationToken = default)
        {
            StoredRooms.Add(room);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken = default)
        {
            var room = StoredRooms.FirstOrDefault(r => r.Code == code);
            return Task.FromResult(room);
        }

        public Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Room>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Room>>(StoredRooms);
        }
    }

    private sealed class TestGuidGenerator : IGuidGenerator
    {
        private readonly Queue<Guid> _predefinedIds;

        public TestGuidGenerator(IEnumerable<Guid>? ids = null)
        {
            _predefinedIds = ids is null ? new Queue<Guid>() : new Queue<Guid>(ids);
        }

        public Guid Create()
        {
            if (_predefinedIds.Count > 0)
            {
                return _predefinedIds.Dequeue();
            }

            return Guid.NewGuid();
        }

        public void Enqueue(Guid identifier)
        {
            _predefinedIds.Enqueue(identifier);
        }
    }

    private sealed class TestClock : IClock
    {
        private DateTime _utcNow;

        public TestClock(DateTime? initial = null)
        {
            var timestamp = initial ?? DateTime.UtcNow;
            _utcNow = timestamp.Kind == DateTimeKind.Utc
                ? timestamp
                : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        }

        public DateTime UtcNow
        {
            get
            {
                var current = _utcNow;
                _utcNow = _utcNow.AddMilliseconds(1);
                return current;
            }
        }

        public void Set(DateTime value)
        {
            _utcNow = value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public void Advance(TimeSpan interval)
        {
            _utcNow = _utcNow.Add(interval);
        }
    }
}
