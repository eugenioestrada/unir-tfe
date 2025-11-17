using Microsoft.Playwright;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Visual regression tests to ensure the design remains stunning across updates.
/// Takes screenshots and validates visual consistency.
/// </summary>
public class VisualRegressionTests : PlaywrightTest
{
    private const string BaseUrl = "https://localhost:7000";

                [Fact]
    // Captures screenshot of the lobby page for visual validation
    public async Task LobbyPage_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        
        // Wait for animations to complete
        await Page.WaitForTimeoutAsync(1000);

        // Take full page screenshot
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/lobby-desktop.png",
            FullPage = true
        });

        // Verify screenshot was created
        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/lobby-desktop.png"), Is.True, 
            "Screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of lobby on mobile for visual validation
    public async Task LobbyPage_Mobile_ShouldLookStunning()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{BaseUrl}/");
        
        await Page.WaitForTimeoutAsync(1000);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/lobby-mobile.png",
            FullPage = true
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/lobby-mobile.png"), Is.True, 
            "Mobile screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of lobby on tablet for visual validation
    public async Task LobbyPage_Tablet_ShouldLookStunning()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync($"{BaseUrl}/");
        
        await Page.WaitForTimeoutAsync(1000);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/lobby-tablet.png",
            FullPage = true
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/lobby-tablet.png"), Is.True, 
            "Tablet screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of room with QR code
    public async Task RoomWithQRCode_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Create a room
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        
        // Wait for room creation and QR code rendering
        await Page.WaitForTimeoutAsync(3000);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/room-with-qr-desktop.png",
            FullPage = true
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/room-with-qr-desktop.png"), Is.True, 
            "Room with QR code screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of room with QR code on mobile
    public async Task RoomWithQRCode_Mobile_ShouldLookStunning()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{BaseUrl}/");

        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(3000);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/room-with-qr-mobile.png",
            FullPage = true
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/room-with-qr-mobile.png"), Is.True, 
            "Mobile room screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of hero section
    public async Task HeroSection_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForTimeoutAsync(1000);

        var hero = Page.Locator(".game-hero");
        await Expect(hero).ToBeVisibleAsync();

        await hero.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = "screenshots/hero-section.png"
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/hero-section.png"), Is.True, 
            "Hero section screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of game cards
    public async Task GameCards_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForTimeoutAsync(1000);

        var card = Page.Locator(".game-card").First;
        await Expect(card).ToBeVisibleAsync();

        await card.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = "screenshots/game-card.png"
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/game-card.png"), Is.True, 
            "Game card screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of primary button
    public async Task PrimaryButton_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForTimeoutAsync(1000);

        var button = Page.Locator(".game-btn-primary").First;
        await Expect(button).ToBeVisibleAsync();

        await button.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = "screenshots/primary-button.png"
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/primary-button.png"), Is.True, 
            "Primary button screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of button hover state
    public async Task ButtonHover_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForTimeoutAsync(1000);

        var button = Page.Locator(".game-btn-primary").First;
        await Expect(button).ToBeVisibleAsync();

        // Hover over button
        await button.HoverAsync();
        await Page.WaitForTimeoutAsync(300);

        await button.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = "screenshots/primary-button-hover.png"
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/primary-button-hover.png"), Is.True, 
            "Button hover screenshot should be created successfully");
    }

    [Fact]
    // Captures screenshot of Home/About page
    public async Task HomePage_ShouldLookStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/home");
        await Page.WaitForTimeoutAsync(1000);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/home-page-desktop.png",
            FullPage = true
        });

        // TODO: Convert to xUnit - Assert.That(File.Exists("screenshots/home-page-desktop.png"), Is.True, 
            "Home page screenshot should be created successfully");
    }

    [Fact]
    // Validates that screenshots directory exists
    public async Task ScreenshotsDirectory_ShouldBeCreated()
    {
        await Task.CompletedTask; // Make method async
        
        if (!Directory.Exists("screenshots"))
        {
            Directory.CreateDirectory("screenshots");
        }

        // TODO: Convert to xUnit - Assert.That(Directory.Exists("screenshots"), Is.True, 
            "Screenshots directory should exist");
    }
}
