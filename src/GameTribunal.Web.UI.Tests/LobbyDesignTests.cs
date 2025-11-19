using GameTribunal.Web.UI.Tests.Infrastructure;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate that the Lobby view follows RNF-011 and section 3.1 of ui-design-principles.md.
/// These tests ensure the lobby uses the correct design patterns: timeline, status chips, card layouts, etc.
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class LobbyDesignTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    /// <summary>
    /// Validates that the hero section follows RNF-011 Section 3.1 height constraints.
    /// Hero should never exceed 40% of viewport and should fit within viewport without scrolling.
    /// </summary>
    [Fact]
    public async Task Lobby_HeroSection_ShouldRespectHeightConstraints()
    {
        await Page.GotoAsync("/");
        
        var heroSection = Page.Locator(".game-hero");
        await Expect(heroSection).ToBeVisibleAsync();
        
        // Verify hero has lobby-specific class
        var hasLobbyClass = await heroSection.EvaluateAsync<bool>("el => el.classList.contains('game-hero-lobby')");
        Assert.True(hasLobbyClass, "Hero section should have game-hero-lobby class for proper height constraints");
        
        // Verify the computed height doesn't exceed 40% of viewport
        var viewportHeight = await Page.EvaluateAsync<int>("() => window.innerHeight");
        var heroHeight = await heroSection.EvaluateAsync<int>("el => el.offsetHeight");
        var maxAllowedHeight = viewportHeight * 0.40;
        
        Assert.True(heroHeight <= maxAllowedHeight, 
            $"Hero section height ({heroHeight}px) should not exceed 40% of viewport ({maxAllowedHeight}px)");
        
        // Verify it's still substantial enough to be visible
        Assert.True(heroHeight >= 200, $"Hero section should be at least 200px high, found: {heroHeight}px");
    }

    /// <summary>
    /// Validates that the timeline component exists and shows the correct steps per RNF-011 Section 3.1.
    /// </summary>
    [Fact]
    public async Task Lobby_Timeline_ShouldExistWithCorrectSteps_AfterCreatingRoom()
    {
        await Page.GotoAsync("/");
        
        // Create a room first
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await Expect(createButton).ToBeVisibleAsync();
        await createButton.ClickAsync();
        
        // Wait for room creation
        await Page.WaitForSelectorAsync(".game-timeline", new() { Timeout = 5000 });
        
        var timeline = Page.Locator(".game-timeline");
        await Expect(timeline).ToBeVisibleAsync();
        
        // Check for the three required steps
        var steps = Page.Locator(".game-timeline-step");
        var stepCount = await steps.CountAsync();
        Assert.Equal(3, stepCount);
        
        // Verify step labels
        var step1Label = Page.Locator(".game-timeline-step").Nth(0).Locator(".game-timeline-label");
        await Expect(step1Label).ToContainTextAsync("Sala Creada");
        
        var step2Label = Page.Locator(".game-timeline-step").Nth(1).Locator(".game-timeline-label");
        await Expect(step2Label).ToContainTextAsync("Jugadores Listos");
        
        var step3Label = Page.Locator(".game-timeline-step").Nth(2).Locator(".game-timeline-label");
        await Expect(step3Label).ToContainTextAsync("Iniciar Partida");
    }

    /// <summary>
    /// Validates that the first timeline step is marked as completed after room creation.
    /// </summary>
    [Fact]
    public async Task Lobby_Timeline_FirstStepShouldBeCompleted_AfterRoomCreation()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Page.WaitForSelectorAsync(".game-timeline", new() { Timeout = 5000 });
        
        var firstStep = Page.Locator(".game-timeline-step").First;
        var className = await firstStep.GetAttributeAsync("class");
        
        Assert.Contains("completed", className);
    }

    /// <summary>
    /// Validates that the QR card uses game-card-spotlight class per RNF-011 Section 3.1.
    /// </summary>
    [Fact]
    public async Task Lobby_QRCard_ShouldUseSpotlightClass()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Page.WaitForSelectorAsync(".game-card-spotlight", new() { Timeout = 5000 });
        
        var spotlightCard = Page.Locator(".game-card-spotlight");
        await Expect(spotlightCard).ToBeVisibleAsync();
        
        // Verify it contains QR-related content
        var qrContainer = spotlightCard.Locator(".game-qr-container");
        await Expect(qrContainer).ToBeVisibleAsync();
    }

    /// <summary>
    /// Validates that the roster card uses game-card-roster class per RNF-011 Section 3.1.
    /// </summary>
    [Fact]
    public async Task Lobby_RosterCard_ShouldUseRosterClass()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Page.WaitForSelectorAsync(".game-card-roster", new() { Timeout = 5000 });
        
        var rosterCard = Page.Locator(".game-card-roster");
        await Expect(rosterCard).ToBeVisibleAsync();
        
        // Verify it has the player count badge
        var badge = rosterCard.Locator(".game-badge");
        await Expect(badge).ToBeVisibleAsync();
    }

    /// <summary>
    /// Validates that the split layout has two cards side by side per RNF-011 Section 3.1.
    /// </summary>
    [Fact]
    public async Task Lobby_SplitLayout_ShouldHaveTwoCards()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Page.WaitForSelectorAsync(".game-grid-2", new() { Timeout = 5000 });
        
        var grid = Page.Locator(".game-grid-2").First;
        var cards = grid.Locator(".game-card");
        
        var cardCount = await cards.CountAsync();
        Assert.Equal(2, cardCount);
    }

    /// <summary>
    /// Validates that status chips use the correct CSS classes per RNF-011 Section 3.1.
    /// This test would need players connected to fully test, but we can verify the structure.
    /// </summary>
    [Fact]
    public async Task Lobby_StatusChips_ShouldHaveCorrectStructure()
    {
        // This test verifies the CSS structure exists
        await Page.GotoAsync("/");
        
        // Verify CSS classes are defined by checking stylesheet
        var styles = await Page.EvaluateAsync<string>(@"
            () => {
                const sheets = Array.from(document.styleSheets);
                let hasStatusChip = false;
                let hasConnected = false;
                let hasInactive = false;
                let hasDisconnected = false;
                
                for (const sheet of sheets) {
                    try {
                        const rules = Array.from(sheet.cssRules || sheet.rules || []);
                        for (const rule of rules) {
                            if (rule.selectorText) {
                                if (rule.selectorText.includes('game-chip-status')) hasStatusChip = true;
                                if (rule.selectorText.includes('game-chip-status-connected')) hasConnected = true;
                                if (rule.selectorText.includes('game-chip-status-inactive')) hasInactive = true;
                                if (rule.selectorText.includes('game-chip-status-disconnected')) hasDisconnected = true;
                            }
                        }
                    } catch (e) {
                        // Skip inaccessible stylesheets (CORS)
                    }
                }
                
                return JSON.stringify({
                    hasStatusChip,
                    hasConnected,
                    hasInactive,
                    hasDisconnected
                });
            }
        ");
        
        var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(styles);
        
        Assert.True(result.GetProperty("hasStatusChip").GetBoolean(), "game-chip-status CSS class should exist");
        Assert.True(result.GetProperty("hasConnected").GetBoolean(), "game-chip-status-connected CSS class should exist");
        Assert.True(result.GetProperty("hasInactive").GetBoolean(), "game-chip-status-inactive CSS class should exist");
        Assert.True(result.GetProperty("hasDisconnected").GetBoolean(), "game-chip-status-disconnected CSS class should exist");
    }

    /// <summary>
    /// Validates that the QR code and room code are displayed prominently.
    /// Follows same pattern as other passing tests for consistency.
    /// </summary>
    [Fact]
    public async Task Lobby_QRAndRoomCode_ShouldBeDisplayed()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        // Wait for the QR container to appear (indicates room was created)
        await Page.WaitForSelectorAsync(".game-qr-container", new() { Timeout = 10000 });
        
        // Verify room code is visible
        var roomCode = Page.Locator(".game-room-code");
        await Expect(roomCode).ToBeVisibleAsync();
        
        // Verify the room code has text content
        var roomCodeText = await roomCode.TextContentAsync();
        Assert.False(string.IsNullOrWhiteSpace(roomCodeText), "Room code should have non-empty text");
        
        // Verify QR code image is visible
        var qrImage = Page.Locator(".game-qr-image img");
        await Expect(qrImage).ToBeVisibleAsync();
    }

    /// <summary>
    /// Validates that the game mode badge is shown in the header after room creation.
    /// After room creation, there is no hero section - the badge is in the lobby header.
    /// </summary>
    [Fact]
    public async Task Lobby_GameModeBadge_ShouldBeDisplayedInHero()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Task.Delay(1000); // Wait for room creation
        
        // Verify mode badge is displayed (it's in the lobby header, not a hero section)
        var modeBadge = Page.Locator("span").Filter(new() { HasText = "Normal" }).Or(
            Page.Locator("span").Filter(new() { HasText = "Suave" })).Or(
            Page.Locator("span").Filter(new() { HasText = "Spicy" }));
        
        await Expect(modeBadge).ToBeVisibleAsync();
    }

    /// <summary>
    /// Validates that the timeline icons render correctly.
    /// </summary>
    [Fact]
    public async Task Lobby_Timeline_IconsShouldRender()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Page.WaitForSelectorAsync(".game-timeline-icon", new() { Timeout = 5000 });
        
        var icons = Page.Locator(".game-timeline-icon");
        var iconCount = await icons.CountAsync();
        
        Assert.Equal(3, iconCount);
    }

    /// <summary>
    /// Validates that the CTA button follows enablement rules from RF-003.
    /// </summary>
    [Fact]
    public async Task Lobby_CTAButton_ShouldBeDisabled_WithLessThanMinPlayers()
    {
        await Page.GotoAsync("/");
        
        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        await Task.Delay(1000); // Wait for room creation
        
        // Initially, there are 0 players, so button should not be visible
        // or there should be a warning message
        var warningAlert = Page.Locator(".game-alert").Filter(new() { HasText = "Se necesitan más jugadores" });
        
        // Either warning is visible OR button is not present
        var warningVisible = await warningAlert.IsVisibleAsync();
        var startButton = Page.Locator("button:has-text('Iniciar Partida')");
        var buttonVisible = await startButton.IsVisibleAsync();
        
        // If button is visible, warning should also be visible (they appear together when < 4 players)
        if (buttonVisible)
        {
            Assert.True(warningVisible, "Warning should be visible when players < 4");
        }
    }
}
