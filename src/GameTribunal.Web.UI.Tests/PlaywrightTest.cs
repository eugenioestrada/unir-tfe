using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Base class for Playwright tests using xUnit
/// </summary>
public class PlaywrightTest : IAsyncLifetime
{
    private IPlaywright? _playwright;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = "https://localhost:7000",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (Context != null) await Context.CloseAsync();
        if (Browser != null) await Browser.CloseAsync();
        _playwright?.Dispose();
    }

    protected async Task<string> WaitForComputedStyleAsync(ILocator locator, string propertyName, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
        string value = string.Empty;

        while (DateTime.UtcNow < deadline)
        {
            value = await locator.EvaluateAsync<string>($"el => window.getComputedStyle(el).{propertyName}");
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            await Task.Delay(100);
        }

        return value.Trim();
    }
}
