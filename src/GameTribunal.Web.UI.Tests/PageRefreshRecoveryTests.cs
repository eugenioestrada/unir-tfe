using GameTribunal.Web.UI.Tests.Infrastructure;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// UI tests to validate RF-017: Page refresh recovery for both host and players.
/// Verifies that after refreshing the page, both host and players recover their corresponding view.
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class PageRefreshRecoveryTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    /// <summary>
    /// RF-017: Validates that a player can refresh their page and return to the joined state.
    /// Strategy: Join as a player, refresh the page, verify they're still in the room.
    /// </summary>
    [Fact]
    public async Task Player_RefreshPage_RecoverJoinedState()
    {
        // Step 1: Create a room in the host page
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var gameModeSelect = Page.Locator("select#gameMode");
        await Expect(gameModeSelect).ToBeVisibleAsync();
        await gameModeSelect.SelectOptionAsync("Normal");
        
        var createButton = Page.Locator("button", new() { HasText = "Crear Sala" });
        await createButton.ClickAsync();
        
        // Wait for room to be created
        var roomCodeDisplay = Page.Locator(".game-room-code");
        await Expect(roomCodeDisplay).ToBeVisibleAsync(new() { Timeout = 15000 });
        
        var roomCode = await roomCodeDisplay.InnerTextAsync();
        roomCode = roomCode.Trim();
        
        // Step 2: Open a new page to join as a player
        var playerContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 375, Height = 667 }
        });
        var playerPage = await playerContext.NewPageAsync();
        
        try
        {
            // Step 3: Join the room as a player
            await playerPage.GotoAsync($"/join/{roomCode}");
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var aliasInput = playerPage.Locator("input#alias");
            await Expect(aliasInput).ToBeVisibleAsync();
            await aliasInput.FillAsync("TestPlayer");
            
            var joinButton = playerPage.Locator("button", new() { HasText = "Unirse a la Sala" });
            await Expect(joinButton).ToBeEnabledAsync(new() { Timeout = 5000 });
            await joinButton.ClickAsync();
            
            // Wait for join success
            var successAlert = playerPage.Locator("text=¡Bienvenido/a!");
            await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });
            
            // Verify player badge is visible
            var playerBadge = playerPage.Locator("text=Tú");
            await Expect(playerBadge).ToBeVisibleAsync();
            
            // Take screenshot before refresh
            await playerPage.ScreenshotAsync(new() { Path = "/tmp/test-rf017-player-before-refresh.png" });
            
            // Step 4: Refresh the player page (RF-017)
            await playerPage.ReloadAsync();
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait a bit for the session restoration logic to run
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            // Step 5: Verify the player is still in the joined state (RF-017)
            // Should see the success message and player badge again
            var successAlertAfterRefresh = playerPage.Locator("text=¡Bienvenido/a!");
            await Expect(successAlertAfterRefresh).ToBeVisibleAsync(new() { Timeout = 10000 });
            
            var playerBadgeAfterRefresh = playerPage.Locator("text=Tú");
            await Expect(playerBadgeAfterRefresh).ToBeVisibleAsync();
            
            // Verify player list is visible
            var playerList = playerPage.Locator(".game-player-list");
            await Expect(playerList).ToBeVisibleAsync();
            
            // Take screenshot after refresh
            await playerPage.ScreenshotAsync(new() { Path = "/tmp/test-rf017-player-after-refresh.png" });
        }
        finally
        {
            if (!playerPage.IsClosed)
            {
                await playerPage.CloseAsync();
            }
            await playerContext.CloseAsync();
        }
    }

    /// <summary>
    /// RF-017: Validates that the host can refresh their page and return to the lobby with the room.
    /// Strategy: Create a room as host, refresh the page, verify the room is still displayed.
    /// </summary>
    [Fact]
    public async Task Host_RefreshPage_RecoverRoomState()
    {
        // Step 1: Create a room as host
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var gameModeSelect = Page.Locator("select#gameMode");
        await Expect(gameModeSelect).ToBeVisibleAsync();
        await gameModeSelect.SelectOptionAsync("Spicy");
        
        var createButton = Page.Locator("button", new() { HasText = "Crear Sala" });
        await createButton.ClickAsync();
        
        // Wait for room to be created
        var roomCodeDisplay = Page.Locator(".game-room-code");
        await Expect(roomCodeDisplay).ToBeVisibleAsync(new() { Timeout = 15000 });
        
        var roomCode = await roomCodeDisplay.InnerTextAsync();
        roomCode = roomCode.Trim();
        
        // Verify QR code is visible
        var qrImage = Page.Locator("img[alt='Código QR para unirse']");
        await Expect(qrImage).ToBeVisibleAsync();
        
        // Take screenshot before refresh
        await Page.ScreenshotAsync(new() { Path = "/tmp/test-rf017-host-before-refresh.png" });
        
        // Step 2: Refresh the host page (RF-017)
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait a bit for the session restoration logic to run
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        // Step 3: Verify the room is still displayed (RF-017)
        var roomCodeDisplayAfterRefresh = Page.Locator(".game-room-code");
        await Expect(roomCodeDisplayAfterRefresh).ToBeVisibleAsync(new() { Timeout = 10000 });
        
        var roomCodeAfterRefresh = await roomCodeDisplayAfterRefresh.InnerTextAsync();
        roomCodeAfterRefresh = roomCodeAfterRefresh.Trim();
        
        // Verify the room code matches
        Assert.Equal(roomCode, roomCodeAfterRefresh);
        
        // Verify QR code is still visible
        var qrImageAfterRefresh = Page.Locator("img[alt='Código QR para unirse']");
        await Expect(qrImageAfterRefresh).ToBeVisibleAsync();
        
        // Verify the game mode badge shows Spicy
        var spicyBadge = Page.Locator("text=🔥 Spicy");
        await Expect(spicyBadge).ToBeVisibleAsync();
        
        // Take screenshot after refresh
        await Page.ScreenshotAsync(new() { Path = "/tmp/test-rf017-host-after-refresh.png" });
    }

    /// <summary>
    /// RF-017: Validates that both host and player can refresh independently and maintain their states.
    /// Strategy: Create room as host, join as player, both refresh, verify both maintain their views.
    /// </summary>
    [Fact]
    public async Task HostAndPlayer_BothRefreshPage_MaintainTheirStates()
    {
        // Step 1: Create a room as host
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var gameModeSelect = Page.Locator("select#gameMode");
        await Expect(gameModeSelect).ToBeVisibleAsync();
        await gameModeSelect.SelectOptionAsync("Normal");
        
        var createButton = Page.Locator("button", new() { HasText = "Crear Sala" });
        await createButton.ClickAsync();
        
        var roomCodeDisplay = Page.Locator(".game-room-code");
        await Expect(roomCodeDisplay).ToBeVisibleAsync(new() { Timeout = 15000 });
        
        var roomCode = await roomCodeDisplay.InnerTextAsync();
        roomCode = roomCode.Trim();
        
        // Step 2: Join as a player
        var playerContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 375, Height = 667 }
        });
        var playerPage = await playerContext.NewPageAsync();
        
        try
        {
            await playerPage.GotoAsync($"/join/{roomCode}");
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var aliasInput = playerPage.Locator("input#alias");
            await aliasInput.FillAsync("RefreshPlayer");
            
            var joinButton = playerPage.Locator("button", new() { HasText = "Unirse a la Sala" });
            await joinButton.ClickAsync();
            
            var successAlert = playerPage.Locator("text=¡Bienvenido/a!");
            await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });
            
            // Step 3: Host refreshes
            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            // Verify host still sees their room
            var hostRoomCode = Page.Locator(".game-room-code");
            await Expect(hostRoomCode).ToBeVisibleAsync(new() { Timeout = 10000 });
            
            // Step 4: Player refreshes
            await playerPage.ReloadAsync();
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            // Verify player still in joined state
            var playerSuccessAlert = playerPage.Locator("text=¡Bienvenido/a!");
            await Expect(playerSuccessAlert).ToBeVisibleAsync(new() { Timeout = 10000 });
            
            var playerBadge = playerPage.Locator("text=Tú");
            await Expect(playerBadge).ToBeVisibleAsync();
            
            // Step 5: Take final screenshots
            await Page.ScreenshotAsync(new() { Path = "/tmp/test-rf017-host-and-player-host-final.png" });
            await playerPage.ScreenshotAsync(new() { Path = "/tmp/test-rf017-host-and-player-player-final.png" });
        }
        finally
        {
            if (!playerPage.IsClosed)
            {
                await playerPage.CloseAsync();
            }
            await playerContext.CloseAsync();
        }
    }

    /// <summary>
    /// RF-017: Validates that refreshing without a session does not break the page.
    /// Strategy: Navigate to join page without joining, refresh, verify page renders correctly.
    /// </summary>
    [Fact]
    public async Task Player_RefreshWithoutSession_ShowsJoinForm()
    {
        // Step 1: Create a room to get a valid room code
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var gameModeSelect = Page.Locator("select#gameMode");
        await gameModeSelect.SelectOptionAsync("Normal");
        
        var createButton = Page.Locator("button", new() { HasText = "Crear Sala" });
        await createButton.ClickAsync();
        
        var roomCodeDisplay = Page.Locator(".game-room-code");
        await Expect(roomCodeDisplay).ToBeVisibleAsync(new() { Timeout = 15000 });
        
        var roomCode = await roomCodeDisplay.InnerTextAsync();
        roomCode = roomCode.Trim();
        
        // Step 2: Open player page but don't join
        var playerContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 375, Height = 667 }
        });
        var playerPage = await playerContext.NewPageAsync();
        
        try
        {
            await playerPage.GotoAsync($"/join/{roomCode}");
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Verify join form is visible
            var aliasInput = playerPage.Locator("input#alias");
            await Expect(aliasInput).ToBeVisibleAsync();
            
            // Step 3: Refresh without joining (RF-017)
            await playerPage.ReloadAsync();
            await playerPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Step 4: Verify join form is still visible
            var aliasInputAfterRefresh = playerPage.Locator("input#alias");
            await Expect(aliasInputAfterRefresh).ToBeVisibleAsync();
            
            var joinButtonAfterRefresh = playerPage.Locator("button", new() { HasText = "Unirse a la Sala" });
            await Expect(joinButtonAfterRefresh).ToBeVisibleAsync();
            
            // Verify room code is still displayed
            var roomCodeInForm = playerPage.Locator(".game-room-code");
            await Expect(roomCodeInForm).ToHaveTextAsync(roomCode);
        }
        finally
        {
            if (!playerPage.IsClosed)
            {
                await playerPage.CloseAsync();
            }
            await playerContext.CloseAsync();
        }
    }

    /// <summary>
    /// RF-017: Validates that refreshing the lobby without a room shows the create room form.
    /// Strategy: Navigate to lobby, refresh without creating room, verify create form is visible.
    /// </summary>
    [Fact]
    public async Task Host_RefreshWithoutRoom_ShowsCreateForm()
    {
        // Step 1: Navigate to lobby
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Verify create room form is visible
        var gameModeSelect = Page.Locator("select#gameMode");
        await Expect(gameModeSelect).ToBeVisibleAsync();
        
        var createButton = Page.Locator("button", new() { HasText = "Crear Sala" });
        await Expect(createButton).ToBeVisibleAsync();
        
        // Step 2: Refresh without creating room (RF-017)
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Step 3: Verify create room form is still visible
        var gameModeSelectAfterRefresh = Page.Locator("select#gameMode");
        await Expect(gameModeSelectAfterRefresh).ToBeVisibleAsync();
        
        var createButtonAfterRefresh = Page.Locator("button", new() { HasText = "Crear Sala" });
        await Expect(createButtonAfterRefresh).ToBeVisibleAsync();
    }
}
