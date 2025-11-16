using GameTribunal.Application.Contracts;
using GameTribunal.Application.Services;
using GameTribunal.Domain.Entities;
using GameTribunal.Domain.Enumerations;
using GameTribunal.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
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
        var service = new RoomService(repository, generator, NullLogger<RoomService>.Instance);

        var result = await service.CreateRoomAsync(GameMode.Suave);

        Assert.Equal(code.Value, result.Code);
        Assert.Equal(GameMode.Suave, result.Mode);
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
        var service = new RoomService(repository, generator, NullLogger<RoomService>.Instance);

        var result = await service.CreateRoomAsync(GameMode.Normal);

        Assert.Equal(freshCode.Value, result.Code);
        Assert.Contains(repository.StoredRooms, room => room.Code == freshCode);
        Assert.True(generator.GeneratedCodes.Count == 2, "RoomService should have requested a second code on collision.");
    }

    [Fact]
    public async Task CreateRoomAsync_ThrowsWhenNoUniqueCodeAvailable()
    {
        var conflictingCode = RoomCode.From("ABC123");
        var repository = new AlwaysConflictingRoomRepository();
        var generator = new StubRoomCodeGenerator(Enumerable.Repeat(conflictingCode, 20).ToArray());
        var service = new RoomService(repository, generator, NullLogger<RoomService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRoomAsync(GameMode.Spicy));
        Assert.Empty(repository.StoredRooms);
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
    }
}
