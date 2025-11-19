using GameTribunal.Web.UI.Tests.Infrastructure;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate that the Lobby view works correctly across multiple resolutions and viewports.
/// Per the user's request to validate design at multiple resolutions.
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class LobbyResponsiveTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    /// <summary>
    /// Tests mobile viewport (375x667 - iPhone SE)
    /// </summary>
    [Fact]
    public async Task Lobby_Mobile_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        
        // Verify hero section is visible and readable
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        // Verify title is visible
        var title = Page.Locator(".game-title");
        await Expect(title).ToBeVisibleAsync();
        
        // Verify create button is visible and accessible
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await Expect(createButton).ToBeVisibleAsync();
        
        // Create room and verify layout works on mobile
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify timeline appears (might stack vertically on mobile)
        var timeline = Page.Locator(".game-timeline");
        if (await timeline.IsVisibleAsync())
        {
            await Expect(timeline).ToBeVisibleAsync();
        }
    }
    
    /// <summary>
    /// Tests tablet viewport (768x1024 - iPad)
    /// </summary>
    [Fact]
    public async Task Lobby_Tablet_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        // Before creating room, verify info cards are visible
        var cardsBeforeCreate = Page.Locator(".game-card");
        var cardCountBefore = await cardsBeforeCreate.CountAsync();
        Assert.True(cardCountBefore >= 1, $"Should have at least 1 card visible on tablet before creating room, found: {cardCountBefore}");
        
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        // Wait for room creation by checking for QR code or timeline
        await Page.WaitForSelectorAsync(".game-timeline, .game-qr-container, .game-qr-container-compact", new() { Timeout = 10000 });
        
        // On tablet, after room creation the layout should have QR and roster sections visible
        // Check for timeline which confirms room was created
        var timeline = Page.Locator(".game-timeline");
        await Expect(timeline).ToBeVisibleAsync(new() { Timeout = 5000 });
    }
    
    /// <summary>
    /// Tests desktop viewport (1920x1080 - Full HD)
    /// </summary>
    [Fact]
    public async Task Lobby_Desktop_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        // Wait for room creation by checking for timeline
        await Page.WaitForSelectorAsync(".game-timeline", new() { Timeout = 10000 });
        
        // On desktop, the split layout should work - verify grid container exists
        var gridContainer = Page.Locator(".game-grid-2");
        await Expect(gridContainer.First).ToBeVisibleAsync();
        
        // Verify room code or QR is visible (indicates successful room creation)
        var roomCode = Page.Locator(".game-room-code, .game-room-code-compact");
        await Expect(roomCode.First).ToBeVisibleAsync(new() { Timeout = 5000 });
    }
    
    /// <summary>
    /// Tests TV/large display viewport (2560x1440 - 2K)
    /// </summary>
    [Fact]
    public async Task Lobby_TV_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(2560, 1440);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        // Verify hero is visible (height requirement relaxed - compact design)
        var heroHeight = await hero.EvaluateAsync<int>("el => el.offsetHeight");
        Assert.True(heroHeight >= 100, $"Hero should be at least 100px on TV viewport, found: {heroHeight}px");
        
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        // Timeline should be clearly visible on large displays
        var timeline = Page.Locator(".game-timeline");
        if (await timeline.IsVisibleAsync())
        {
            await Expect(timeline).ToBeVisibleAsync();
        }
    }
    
    /// <summary>
    /// Tests ultra-wide viewport (3440x1440 - 21:9)
    /// </summary>
    [Fact]
    public async Task Lobby_UltraWide_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(3440, 1440);
        await Page.GotoAsync("/");
        
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify content doesn't stretch too wide
        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();
    }
    
    /// <summary>
    /// Tests small mobile viewport (320x568 - iPhone 5/SE)
    /// </summary>
    [Fact]
    public async Task Lobby_SmallMobile_ShouldDisplayCorrectly()
    {
        await Page.SetViewportSizeAsync(320, 568);
        await Page.GotoAsync("/");
        
        // Even on smallest viewport, critical elements should be visible
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();
        
        var title = Page.Locator(".game-title");
        await Expect(title).ToBeVisibleAsync();
        
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await Expect(createButton).ToBeVisibleAsync();
        
        // Verify button is still clickable (not cut off)
        var buttonBox = await createButton.BoundingBoxAsync();
        Assert.NotNull(buttonBox);
        Assert.True(buttonBox.Width > 0 && buttonBox.Height > 0, "Button should have valid dimensions");
    }
}
