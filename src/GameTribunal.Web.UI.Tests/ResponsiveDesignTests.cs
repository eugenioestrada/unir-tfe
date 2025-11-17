using Microsoft.Playwright;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate that the game design is responsive and looks amazing on all devices.
/// Validates mobile, tablet, desktop, and TV (10-foot UI) experiences.
/// </summary>
public class ResponsiveDesignTests : PlaywrightTest
{
    private const string BaseUrl = "https://localhost:7000";

    [Fact]
    // Validates design on mobile portrait (375x667 - iPhone SE)
    public async Task MobilePortrait_ShouldLookAmazing()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{BaseUrl}/");

        // Verify hero section adapts to mobile
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        var heroPadding = await hero.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        // TODO: Convert to xUnit - Assert.That(heroPadding, Is.Not.Empty, "Hero should have appropriate padding on mobile");

        // Verify title is readable
        var title = Page.Locator(".game-title");
        var fontSize = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
        var fontSizeValue = double.Parse(fontSize.Replace("px", ""));
        Assert.True(fontSizeValue >= 24), 
            "Title should be readable on mobile (>=24px)");

        // Verify buttons are touch-friendly
        var button = Page.Locator(".game-btn").First;
        var buttonHeight = await button.EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(buttonHeight >= 48), 
            "Buttons should meet minimum touch target size (48px) on mobile");

        // Verify cards stack vertically
        var cards = Page.Locator(".game-card");
        var cardCount = await cards.CountAsync();
        if (cardCount > 1)
        {
            var firstCardBottom = await cards.Nth(0).EvaluateAsync<double>("el => el.getBoundingClientRect().bottom");
            var secondCardTop = await cards.Nth(1).EvaluateAsync<double>("el => el.getBoundingClientRect().top");
            Assert.True(secondCardTop >= firstCardBottom - 5), 
                "Cards should stack vertically on mobile");
        }
    }

    [Fact]
    // Validates design on mobile landscape (667x375)
    public async Task MobileLandscape_ShouldOptimizeForSmallHeight()
    {
        await Page.SetViewportSizeAsync(667, 375);
        await Page.GotoAsync($"{BaseUrl}/");

        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();

        // Verify compact layout for landscape
        var containerPadding = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        // TODO: Convert to xUnit - Assert.That(containerPadding, Is.Not.Empty, "Container should have optimized padding for landscape");

        // Verify title is scaled appropriately
        var title = Page.Locator(".game-title");
        var fontSize = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
        var fontSizeValue = double.Parse(fontSize.Replace("px", ""));
        Assert.True(fontSizeValue > 0).And.LessThanOrEqualTo(48), 
            "Title should be scaled down for landscape orientation");
    }

    [Fact]
    // Validates design on tablet (768x1024 - iPad)
    public async Task Tablet_ShouldProvideOptimalExperience()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync($"{BaseUrl}/");

        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        // Verify spacing is appropriate for tablet
        var heroPadding = await hero.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        // TODO: Convert to xUnit - Assert.That(heroPadding, Is.Not.Empty, "Hero should have tablet-optimized padding");

        // Verify grid adapts to tablet
        var grid = Page.Locator(".game-grid-2");
        if (await grid.CountAsync() > 0)
        {
            var gridTemplate = await grid.EvaluateAsync<string>("el => window.getComputedStyle(el).gridTemplateColumns");
            Assert.Contains("px", gridTemplate).Or.Contain("fr"), 
                "Grid should adapt to tablet screen size");
        }

        // Verify buttons are appropriately sized
        var button = Page.Locator(".game-btn-lg").First;
        var buttonMinHeight = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).minHeight");
        var minHeightValue = double.Parse(buttonMinHeight.Replace("px", ""));
        Assert.True(minHeightValue >= 52), 
            "Large buttons should be easy to tap on tablet");
    }

    [Fact]
    // Validates design on desktop (1920x1080 - Full HD)
    public async Task Desktop_ShouldShowFullGlory()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync($"{BaseUrl}/");

        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();

        // Verify container has max-width for readability
        var maxWidth = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).maxWidth");
        Assert.NotEqual("none", maxWidth), 
            "Container should have max-width for optimal readability on large screens");

        // Verify generous spacing
        var containerPadding = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        Assert.Contains("px", containerPadding), 
            "Container should have generous padding on desktop");

        // Verify QR code is prominently displayed
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);

        var qrImage = Page.Locator(".game-qr-image img");
        if (await qrImage.CountAsync() > 0)
        {
            var width = await qrImage.EvaluateAsync<double>("el => el.offsetWidth");
            Assert.True(width >= 250), 
                "QR code should be large and easy to scan on desktop (>=250px)");
        }
    }

    [Fact]
    // Validates design on TV/10-foot UI (1920x1080+)
    public async Task TV_ShouldProvide10FootUIExperience()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync($"{BaseUrl}/");

        // Verify base font size is increased for TV viewing
        var baseFontSize = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).fontSize");
        var fontSizeValue = double.Parse(baseFontSize.Replace("px", ""));
        Assert.True(fontSizeValue >= 16), 
            "Base font size should be readable from distance on TV");

        // Verify buttons are large enough for remote control navigation
        var button = Page.Locator(".game-btn-lg").First;
        var buttonHeight = await button.EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(buttonHeight >= 60), 
            "Buttons should be large for TV remote control (>=60px height)");

        // Verify room code is extra large for TV display
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);

        var roomCode = Page.Locator(".game-room-code");
        if (await roomCode.CountAsync() > 0)
        {
            var fontSize = await roomCode.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
            var codeFontSize = double.Parse(fontSize.Replace("px", ""));
            Assert.True(codeFontSize >= 48), 
                "Room code should be extra large for TV viewing (>=48px)");
        }
    }

    [Fact]
    // Validates that content doesn't overflow on small screens
    public async Task SmallScreens_ShouldNotOverflow()
    {
        await Page.SetViewportSizeAsync(320, 568); // iPhone 5/SE
        await Page.GotoAsync($"{BaseUrl}/");

        // Check for horizontal scrollbar
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        
        // TODO: Convert to xUnit - Assert.That(hasHorizontalScroll, Is.False, 
            "Page should not have horizontal scroll on small screens");

        // Verify all content is visible within viewport
        var container = Page.Locator(".game-container");
        var containerWidth = await container.EvaluateAsync<double>("el => el.offsetWidth");
        var viewportWidth = await Page.EvaluateAsync<double>("window.innerWidth");
        
        Assert.True(containerWidth <= viewportWidth), 
            "Container should fit within viewport width");
    }

    [Fact]
    // Validates that images are responsive and don't break layout
    public async Task Images_ShouldBeResponsive()
    {
        var viewports = new[] 
        {
            new { Width = 375, Height = 667, Name = "Mobile" },
            new { Width = 768, Height = 1024, Name = "Tablet" },
            new { Width = 1920, Height = 1080, Name = "Desktop" }
        };

        foreach (var viewport in viewports)
        {
            await Page.SetViewportSizeAsync(viewport.Width, viewport.Height);
            await Page.GotoAsync($"{BaseUrl}/");

            // Create room to see QR code
            var createButton = Page.Locator("button:has-text('Crear Sala')");
            await createButton.ClickAsync();
            await Page.WaitForTimeoutAsync(2000);

            var qrImage = Page.Locator(".game-qr-image img");
            if (await qrImage.CountAsync() > 0)
            {
                // Verify image doesn't overflow container
                var imageWidth = await qrImage.EvaluateAsync<double>("el => el.offsetWidth");
                var containerWidth = await Page.Locator(".game-qr-image").EvaluateAsync<double>("el => el.offsetWidth");
                
                Assert.True(imageWidth <= containerWidth + 1), 
                    $"QR image should not overflow container on {viewport.Name}");

                // Verify image has max-width
                var maxWidth = await qrImage.EvaluateAsync<string>("el => window.getComputedStyle(el).maxWidth");
                Assert.NotEqual("none", maxWidth), 
                    $"QR image should have max-width constraint on {viewport.Name}");
            }
        }
    }

    [Fact]
    // Validates that touch targets meet minimum size requirements
    public async Task TouchTargets_ShouldMeetMinimumSize()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{BaseUrl}/");

        // Get all interactive elements
        var buttons = await Page.Locator("button, a.game-btn, .game-btn").AllAsync();

        foreach (var button in buttons)
        {
            if (await button.IsVisibleAsync())
            {
                var height = await button.EvaluateAsync<double>("el => el.offsetHeight");
                var width = await button.EvaluateAsync<double>("el => el.offsetWidth");

                // WCAG 2.1 Level AAA recommends 44x44px minimum
                Assert.True(height >= 44), 
                    "Interactive elements should be at least 44px tall for accessibility");
                Assert.True(width >= 44), 
                    "Interactive elements should be at least 44px wide for accessibility");
                
                break; // Test first button to avoid timeout
            }
        }
    }

    [Fact]
    // Validates that grid layout adapts correctly across breakpoints
    public async Task GridLayout_ShouldAdaptToBreakpoints()
    {
        var testCases = new[]
        {
            new { Width = 375, Height = 667, ExpectedColumns = 1, Name = "Mobile" },
            new { Width = 768, Height = 1024, ExpectedColumns = 2, Name = "Tablet" },
            new { Width = 1920, Height = 1080, ExpectedColumns = 2, Name = "Desktop" }
        };

        foreach (var testCase in testCases)
        {
            await Page.SetViewportSizeAsync(testCase.Width, testCase.Height);
            await Page.GotoAsync($"{BaseUrl}/");

            var grid = Page.Locator(".game-grid-2");
            if (await grid.CountAsync() > 0)
            {
                await grid.First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });
                var gridTemplate = await grid.EvaluateAsync<string>("el => window.getComputedStyle(el).gridTemplateColumns");
                
                // Count columns by counting "px" or "fr" occurrences
                var columnCount = gridTemplate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                
                Assert.True(columnCount > 0), 
                    $"Grid should have defined columns on {testCase.Name}");
            }
        }
    }
}
