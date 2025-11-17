using GameTribunal.Web.UI.Tests.Infrastructure;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Base class for Playwright tests using xUnit.
/// </summary>
public abstract class PlaywrightTest : IAsyncLifetime
{
    private readonly TestServerFixture _serverFixture;
    private IPlaywright? _playwright;
    private string? _baseUrl;

    protected PlaywrightTest(TestServerFixture serverFixture)
    {
        ArgumentNullException.ThrowIfNull(serverFixture);
        _serverFixture = serverFixture;
    }

    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BaseUrl => _baseUrl ??= _serverFixture.BaseAddress.ToString().TrimEnd('/');
    
    public async Task InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        }).ConfigureAwait(false);
        
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        }).ConfigureAwait(false);
        
        Page = await Context.NewPageAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync().ConfigureAwait(false);
        }

        if (Context != null)
        {
            await Context.CloseAsync().ConfigureAwait(false);
        }

        if (Browser != null)
        {
            await Browser.CloseAsync().ConfigureAwait(false);
        }

        _playwright?.Dispose();
    }

    protected async Task<string> WaitForComputedStyleAsync(ILocator locator, string propertyName, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
        var value = string.Empty;

        while (DateTime.UtcNow < deadline)
        {
            value = await locator.EvaluateAsync<string>($"el => window.getComputedStyle(el).{propertyName}").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            await Task.Delay(100).ConfigureAwait(false);
        }

        return value.Trim();
    }
}
