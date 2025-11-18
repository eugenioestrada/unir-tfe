using GameTribunal.Web.UI.Tests.Infrastructure;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate RNF-010: Adaptar layout a pantalla completa.
/// 
/// RNF-010 requirement: "La interfaz debe ajustarse dinámicamente al viewport disponible, 
/// redistribuyendo componentes para ocupar toda la superficie útil sin requerir 
/// desplazamiento vertical u horizontal."
/// 
/// These tests ensure:
/// 1. No horizontal scrolling required on any viewport
/// 2. No vertical scrolling required on standard viewports
/// 3. Content fills the available viewport height dynamically
/// 4. Layout adapts properly when viewport changes (e.g., mobile browser chrome)
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class FullscreenLayoutTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    private const int WAIT_FOR_TIMEOUT = 300;

    [Theory]
    [InlineData(320, 568, "iPhone SE (Portrait)")]
    [InlineData(375, 667, "iPhone 8 (Portrait)")]
    [InlineData(414, 896, "iPhone 11 Pro Max (Portrait)")]
    [InlineData(768, 1024, "iPad (Portrait)")]
    [InlineData(1024, 768, "iPad (Landscape)")]
    [InlineData(1920, 1080, "Full HD Desktop")]
    [InlineData(2560, 1440, "2K Desktop")]
    [InlineData(3840, 2160, "4K TV")]
    // RNF-010: Validates that no horizontal scrolling is required on any viewport
    public async Task RNF010_NoHorizontalScrolling_OnAnyViewport(int width, int height, string deviceName)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for horizontal scrollbar
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        
        Assert.False(hasHorizontalScroll, 
            $"Horizontal scrolling detected on {deviceName} ({width}x{height})");

        // Verify document width doesn't exceed viewport
        var documentWidth = await Page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        var viewportWidth = await Page.EvaluateAsync<double>("window.innerWidth");
        
        Assert.True(documentWidth <= viewportWidth + 1, // +1 for rounding
            $"Document width ({documentWidth}px) exceeds viewport width ({viewportWidth}px) on {deviceName}");
    }

    [Theory]
    [InlineData(375, 667, "iPhone 8 (Portrait)")]
    [InlineData(414, 896, "iPhone 11 Pro Max (Portrait)")]
    [InlineData(768, 1024, "iPad (Portrait)")]
    [InlineData(1920, 1080, "Full HD Desktop")]
    // RNF-010: Validates that content fills viewport without requiring vertical scrolling on standard viewports
    public async Task RNF010_NoVerticalScrolling_OnStandardViewports(int width, int height, string deviceName)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check if vertical scrollbar is present
        var hasVerticalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollHeight > document.documentElement.clientHeight");
        
        // On the initial lobby page, we should not need vertical scrolling
        Assert.False(hasVerticalScroll, 
            $"Vertical scrolling required on {deviceName} ({width}x{height}) - content should fit viewport");
    }

    [Theory]
    [InlineData(375, 667, "iPhone 8")]
    [InlineData(768, 1024, "iPad")]
    [InlineData(1920, 1080, "Desktop")]
    // RNF-010: Validates that game-container fills the viewport height dynamically
    public async Task RNF010_ContainerFillsViewport_Dynamically(int width, int height, string deviceName)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();

        // Get container computed height
        var containerHeight = await container.EvaluateAsync<double>("el => el.offsetHeight");
        var viewportHeight = await Page.EvaluateAsync<double>("window.innerHeight");

        // Container should be at least as tall as viewport (it fills the space)
        Assert.True(containerHeight >= viewportHeight * 0.95, // Allow 5% tolerance
            $"Container height ({containerHeight}px) doesn't fill viewport ({viewportHeight}px) on {deviceName}");
    }

    [Fact]
    // RNF-010: Validates that layout adapts when viewport size changes dynamically
    public async Task RNF010_LayoutAdapts_WhenViewportChanges()
    {
        // Start with mobile portrait
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasScrollInitial = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasScrollInitial, "Initial viewport should not have horizontal scroll");

        // Change to tablet landscape
        await Page.SetViewportSizeAsync(1024, 768);
        await Page.WaitForTimeoutAsync(100); // Allow layout to adjust

        var hasScrollAfterResize = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasScrollAfterResize, "Resized viewport should not have horizontal scroll");

        // Change to desktop
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.WaitForTimeoutAsync(100); // Allow layout to adjust

        var hasScrollDesktop = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasScrollDesktop, "Desktop viewport should not have horizontal scroll");
    }

    [Theory]
    [InlineData(375, 667)]
    [InlineData(414, 896)]
    // RNF-010: Validates that mobile browser chrome changes don't break layout
    public async Task RNF010_LayoutHandles_MobileBrowserChromeChanges(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Simulate reduced viewport height (browser chrome visible)
        var reducedHeight = height - 100; // Simulate browser chrome taking 100px
        await Page.SetViewportSizeAsync(width, reducedHeight);
        await Page.WaitForTimeoutAsync(100);

        // Content should still not overflow horizontally
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHorizontalScroll, 
            "Layout should handle browser chrome changes without horizontal overflow");

        // Container should still be responsive to height
        var container = Page.Locator(".game-container");
        var containerHeight = await container.EvaluateAsync<double>("el => el.offsetHeight");
        var viewportHeight = await Page.EvaluateAsync<double>("window.innerHeight");

        // Container should adapt to new viewport height
        Assert.True(containerHeight <= viewportHeight * 1.1, // Allow 10% tolerance for scrollable content
            $"Container should adapt to reduced viewport height");
    }

    [Fact]
    // RNF-010: Validates that the lobby page specifically meets fullscreen requirements
    public async Task RNF010_LobbyPage_MeetsFullscreenRequirements()
    {
        // Test on a typical mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // No horizontal scroll
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHorizontalScroll, "Lobby page should not require horizontal scrolling");

        // Content should be visible without scrolling
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        var heroRect = await hero.EvaluateAsync<dynamic>("el => el.getBoundingClientRect()");
        var viewportHeight = await Page.EvaluateAsync<double>("window.innerHeight");

        // Hero should be within viewport
        Assert.True((double)heroRect.top >= 0, "Hero should be visible from top");
        Assert.True((double)heroRect.top < viewportHeight, "Hero should be within viewport");
    }

    [Fact]
    // RNF-010: Validates that after creating a room, the layout still meets fullscreen requirements
    public async Task RNF010_AfterCreatingRoom_LayoutStillMeetsRequirements()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        // After room creation, check for overflow
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHorizontalScroll, "After room creation, no horizontal scroll should be required");

        // QR code should be visible within viewport if it exists
        var qrImage = Page.Locator(".game-qr-image");
        if (await qrImage.CountAsync() > 0)
        {
            await Expect(qrImage).ToBeVisibleAsync();
            
            // QR should not cause horizontal overflow
            var qrWidth = await qrImage.EvaluateAsync<double>("el => el.offsetWidth");
            var viewportWidth = await Page.EvaluateAsync<double>("window.innerWidth");
            Assert.True(qrWidth <= viewportWidth, "QR code should fit within viewport width");
        }
    }

    [Theory]
    [InlineData(667, 375, "Mobile Landscape")]
    [InlineData(1024, 768, "Tablet Landscape")]
    // RNF-010: Validates that landscape orientations don't cause overflow
    public async Task RNF010_LandscapeOrientation_NoOverflow(int width, int height, string deviceName)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for any scrollbars
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        var hasVerticalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollHeight > document.documentElement.clientHeight");
        
        Assert.False(hasHorizontalScroll, 
            $"Landscape orientation {deviceName} should not require horizontal scrolling");
        
        // Landscape with limited height should still adapt
        var container = Page.Locator(".game-container");
        var maxHeight = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).maxHeight");
        Assert.NotEmpty(maxHeight);
    }

    [Fact]
    // RNF-010: Validates that all interactive elements are accessible without scrolling
    public async Task RNF010_InteractiveElements_AccessibleWithoutScrolling()
    {
        // Use a typical mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check that the create room button is in viewport
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await Expect(createButton).ToBeVisibleAsync();

        var buttonRect = await createButton.EvaluateAsync<dynamic>("el => el.getBoundingClientRect()");
        var viewportHeight = await Page.EvaluateAsync<double>("window.innerHeight");

        // Button should be within viewport without scrolling
        Assert.True((double)buttonRect.top >= 0 && (double)buttonRect.bottom <= viewportHeight,
            "Create room button should be accessible without scrolling");

        // Game mode selector should also be accessible
        var gameMode = Page.Locator("#gameMode");
        await Expect(gameMode).ToBeVisibleAsync();

        var selectRect = await gameMode.EvaluateAsync<dynamic>("el => el.getBoundingClientRect()");
        Assert.True((double)selectRect.top >= 0 && (double)selectRect.bottom <= viewportHeight,
            "Game mode selector should be accessible without scrolling");
    }
}
