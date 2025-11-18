using GameTribunal.Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace GameTribunal.Web.Services;

/// <summary>
/// Background service that monitors player activity and updates connection statuses.
/// Implements RF-014 (Inactivo after 30s) and RF-015 (Desconectado after 5 min).
/// </summary>
public sealed class PlayerStatusMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<Hubs.GameHub> _hubContext;
    private readonly ILogger<PlayerStatusMonitorService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);

    public PlayerStatusMonitorService(
        IServiceScopeFactory scopeFactory,
        IHubContext<Hubs.GameHub> hubContext,
        ILogger<PlayerStatusMonitorService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlayerStatusMonitorService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndUpdatePlayerStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking player statuses.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("PlayerStatusMonitorService stopping.");
    }

    private async Task CheckAndUpdatePlayerStatusesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();

        // Get all rooms (in a real system, you might want to optimize this)
        var allRooms = await roomRepository.GetAllAsync(cancellationToken);

        foreach (var room in allRooms)
        {
            var statusChanged = false;

            foreach (var player in room.Players)
            {
                var previousStatus = player.ConnectionStatus;
                player.UpdateConnectionStatus();

                if (previousStatus != player.ConnectionStatus)
                {
                    statusChanged = true;
                    _logger.LogInformation(
                        "Player {PlayerId} ({Alias}) status changed from {OldStatus} to {NewStatus} in room {RoomCode}.",
                        player.Id, player.Alias, previousStatus, player.ConnectionStatus, room.Code.Value);
                }
            }

            // If any status changed, update the room and broadcast (RF-016)
            if (statusChanged)
            {
                await roomRepository.UpdateAsync(room, cancellationToken);

                // Broadcast updated state to all clients in the room
                var roomDto = new Application.DTOs.RoomDto(
                    room.Code.Value,
                    room.Mode,
                    room.Players.Select(p => new Application.DTOs.PlayerDto(p.Id, p.Alias, p.ConnectionStatus)).ToList(),
                    room.CanStartGame()
                );

                await _hubContext.Clients.Group(room.Code.Value)
                    .SendAsync("RoomStateUpdated", roomDto, cancellationToken);

                _logger.LogInformation("Broadcasted status update for room {RoomCode}.", room.Code.Value);
            }
        }
    }
}
