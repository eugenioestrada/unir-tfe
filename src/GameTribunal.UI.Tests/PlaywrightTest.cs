using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace GameTribunal.UI.Tests;

public class PlaywrightTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = "https://localhost:7000"
        };
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
