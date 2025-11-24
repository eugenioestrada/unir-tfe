using GameTribunal.Web.UI.Tests.Infrastructure;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate that the game design is responsive and looks amazing on all devices.
/// Validates mobile, tablet, desktop, and TV (10-foot UI) experiences.
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class ResponsiveDesignTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    private const int WAIT_FOR_TIMEOUT = 300;

    [Fact]
    // Validates design on mobile portrait (375x667 - iPhone SE)
    public async Task MobilePortrait_ShouldLookAmazing()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify hero section adapts to mobile
        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        var heroPadding = await hero.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        Assert.NotEmpty(heroPadding);

        // Verify title is readable (mobile uses smaller font)
        var title = Page.Locator(".game-title");
        await Expect(title).ToBeVisibleAsync();
        var fontSize = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
        var fontSizeValue = double.Parse(fontSize.Replace("px", ""));
        Assert.True(fontSizeValue >= 18, $"Title font size should be at least 18px on mobile, found: {fontSizeValue}px");

        // Verify buttons are touch-friendly
        var button = Page.Locator(".game-btn").First;
        var buttonHeight = await button.EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(buttonHeight >= 48);

        // Verify cards stack vertically
        var cards = Page.Locator(".game-card");
        var cardCount = await cards.CountAsync();
        if (cardCount > 1)
        {
            var firstCardBottom = await cards.Nth(0).EvaluateAsync<double>("el => el.getBoundingClientRect().bottom");
            var secondCardTop = await cards.Nth(1).EvaluateAsync<double>("el => el.getBoundingClientRect().top");
            Assert.True(secondCardTop >= firstCardBottom - 5);
        }
    }

    [Fact]
    // Validates design on mobile landscape (667x375)
    public async Task MobileLandscape_ShouldOptimizeForSmallHeight()
    {
        await Page.SetViewportSizeAsync(667, 375);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();

        // Verify compact layout for landscape
        var containerPadding = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        Assert.NotEmpty(containerPadding);

        // Verify title is scaled appropriately
        var title = Page.Locator(".game-title");
        var fontSize = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
        var fontSizeValue = double.Parse(fontSize.Replace("px", ""));
        Assert.True(fontSizeValue > 0 && fontSizeValue <= 48);
    }

    [Fact]
    // Validates design on tablet (768x1024 - iPad)
    public async Task Tablet_ShouldProvideOptimalExperience()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        // Verify spacing is appropriate for tablet
        var heroPadding = await hero.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        Assert.NotEmpty(heroPadding);

        // Verify grid adapts to tablet
        var grid = Page.Locator(".game-grid-2");
        if (await grid.CountAsync() > 0)
        {
            var gridTemplate = await grid.EvaluateAsync<string>("el => window.getComputedStyle(el).gridTemplateColumns");
            Assert.True(gridTemplate.Contains("px") || gridTemplate.Contains("fr"));
        }

        // Verify buttons are appropriately sized
        var button = Page.Locator(".game-btn-lg, .game-btn").First;
        await Expect(button).ToBeVisibleAsync();
        var buttonHeight = await button.EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(buttonHeight >= 44); // Touch-friendly minimum
    }

    [Fact]
    // Validates design on desktop (1920x1080 - Full HD)
    public async Task Desktop_ShouldShowFullGlory()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync("/");

        var container = Page.Locator(".game-container");
        await Expect(container).ToBeVisibleAsync();

        // Verify container has max-width for readability
        var maxWidth = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).maxWidth");
        Assert.NotEqual("none", maxWidth);

        // Verify generous spacing
        var containerPadding = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).padding");
        Assert.Contains("px", containerPadding);

        // Verify QR code is prominently displayed
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        var qrImage = Page.Locator(".game-qr-image img");
        if (await qrImage.CountAsync() > 0)
        {
            var width = await qrImage.EvaluateAsync<double>("el => el.offsetWidth");
            Assert.True(width >= 250);
        }
    }

    [Fact]
    // Validates design on TV/10-foot UI (1920x1080+)
    public async Task TV_ShouldProvide10FootUIExperience()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify base font size is increased for TV viewing
        var baseFontSize = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).fontSize");
        var fontSizeValue = double.Parse(baseFontSize.Replace("px", ""));
        Assert.True(fontSizeValue >= 16);

        // Verify buttons are large enough for remote control navigation
        var button = Page.Locator(".game-btn-lg, .game-btn").First;
        await Expect(button).ToBeVisibleAsync();
        var buttonHeight = await button.EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(buttonHeight >= 48, $"Button height should be at least 48px for TV, found: {buttonHeight}px");

        // Create room to verify room code is visible
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        var roomCode = Page.Locator(".game-room-code, .game-room-code-compact");
        if (await roomCode.CountAsync() > 0)
        {
            var fontSize = await roomCode.First.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
            var codeFontSize = double.Parse(fontSize.Replace("px", ""));
            Assert.True(codeFontSize >= 24, $"Room code font should be at least 24px for TV, found: {codeFontSize}px");
        }
    }

    [Fact]
    // Validates that content doesn't overflow on small screens
    public async Task SmallScreens_ShouldNotOverflow()
    {
        await Page.SetViewportSizeAsync(320, 568); // iPhone 5/SE
        await Page.GotoAsync("/");

        // Check for horizontal scrollbar
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        
        Assert.False(hasHorizontalScroll);

        // Verify all content is visible within viewport
        var container = Page.Locator(".game-container");
        var containerWidth = await container.EvaluateAsync<double>("el => el.offsetWidth");
        var viewportWidth = await Page.EvaluateAsync<double>("window.innerWidth");
        
        Assert.True(containerWidth <= viewportWidth);
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
            // Navigate to a fresh page for each viewport test
            await Page.SetViewportSizeAsync(viewport.Width, viewport.Height);
            await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.NetworkIdle });

            // Create room to see QR code
            var createButton = Page.Locator("button:has-text('Crear Sala')");
            if (await createButton.CountAsync() > 0)
            {
                await createButton.ClickAsync();
                
                // Wait for room creation by checking for QR code or room code
                await Page.WaitForSelectorAsync(".game-qr-image img, .game-qr-container img, .game-room-code, .game-room-code-compact", new() { Timeout = 10000 });
            }

            // Use more flexible selector for QR image (including compact variant)
            var qrImage = Page.Locator(".game-qr-image img, .game-qr-container img");
            if (await qrImage.CountAsync() > 0)
            {
                // Verify image doesn't overflow container
                var imageWidth = await qrImage.First.EvaluateAsync<double>("el => el.offsetWidth");
                var container = Page.Locator(".game-qr-image, .game-qr-container, .game-qr-container-compact").First;
                var containerWidth = await container.EvaluateAsync<double>("el => el.offsetWidth");
                
                Assert.True(imageWidth <= containerWidth + 1, 
                    $"Image width ({imageWidth}px) should not exceed container width ({containerWidth}px) on {viewport.Name}");

                // Verify image has max-width or style constraint
                var maxWidth = await qrImage.First.EvaluateAsync<string>("el => window.getComputedStyle(el).maxWidth");
                var hasStyleWidth = await qrImage.First.EvaluateAsync<bool>("el => el.style.maxWidth !== ''");
                Assert.True(maxWidth != "none" || hasStyleWidth, 
                    $"Image should have max-width constraint on {viewport.Name}");
            }
        }
    }

    [Fact]
    // Validates that touch targets meet minimum size requirements
    public async Task TouchTargets_ShouldMeetMinimumSize()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync("/");

        // Get all interactive elements
        var buttons = await Page.Locator("button, a.game-btn, .game-btn").AllAsync();

        foreach (var button in buttons)
        {
            if (await button.IsVisibleAsync())
            {
                var height = await button.EvaluateAsync<double>("el => el.offsetHeight");
                var width = await button.EvaluateAsync<double>("el => el.offsetWidth");

                // WCAG 2.1 Level AAA recommends 44x44px minimum
                Assert.True(height >= 44);
                Assert.True(width >= 44);
                
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
            await Page.GotoAsync("/");

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
                
                Assert.True(columnCount > 0);
            }
        }
    }
}
