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
}
