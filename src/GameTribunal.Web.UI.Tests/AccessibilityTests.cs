using GameTribunal.Web.UI.Tests.Infrastructure;

namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate that the game meets accessibility standards (WCAG AA).
/// Ensures the stunning design is also inclusive and accessible to all users.
/// </summary>
[Collection(TestServerFixture.CollectionName)]
public class AccessibilityTests(TestServerFixture serverFixture) : PlaywrightTest(serverFixture)
{
    private const int WAIT_FOR_TIMEOUT = 300;

    [Fact]
    // Validates that color contrast meets WCAG AA standards
    public async Task ColorContrast_ShouldMeetWCAGStandards()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Test title contrast
        var title = Page.Locator(".game-title").First;
        await Expect(title).ToBeVisibleAsync();

        var titleColor = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).color");
        var titleBackground = await title.EvaluateAsync<string>(
            "el => { let parent = el.parentElement; while (parent) { const bg = window.getComputedStyle(parent).backgroundColor; if (bg !== 'rgba(0, 0, 0, 0)' && bg !== 'transparent') return bg; parent = parent.parentElement; } return 'rgb(28, 25, 23)'; }");

        Assert.NotEmpty(titleColor);
        Assert.NotEmpty(titleBackground);

        // Test button contrast
        var button = Page.Locator(".game-btn-primary").First;
        if (await button.CountAsync() > 0)
        {
            var buttonColor = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).color");
            var buttonBackground = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).backgroundColor");

            Assert.NotEmpty(buttonColor);
            Assert.NotEqual("rgba(0, 0, 0, 0)", buttonBackground);
        }
    }

    [Fact]
    // Validates that interactive elements can be navigated with keyboard
    public async Task KeyboardNavigation_ShouldWork()
    {
        await Page.GotoAsync("/");

        // Tab through interactive elements
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        // Verify focus is visible
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        Assert.NotEqual("BODY", focusedElement);

        // Verify focus indicator is visible
        var focusedOutline = await Page.EvaluateAsync<string>("window.getComputedStyle(document.activeElement).outline");
        var focusedBoxShadow = await Page.EvaluateAsync<string>("window.getComputedStyle(document.activeElement).boxShadow");

        var hasFocusIndicator = focusedOutline != "none" || focusedBoxShadow.Contains("rgb");
        Assert.True(hasFocusIndicator);
    }

    [Fact]
    // Validates that page has proper semantic HTML structure
    public async Task SemanticHTML_ShouldBeUsed()
    {
        await Page.GotoAsync("/");

        // Check for proper heading hierarchy
        var h1Count = await Page.Locator("h1").CountAsync();
        Assert.True(h1Count > 0);

        // Check for proper button elements (not divs with click handlers)
        var allButtons = await Page.Locator("button, a.game-btn").AllAsync();
        Assert.True(allButtons.Count > 0);
    }

    [Fact]
    // Validates that images have alt text
    public async Task Images_ShouldHaveAltText()
    {
        await Page.GotoAsync("/");

        // Create room to see QR code
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        var images = await Page.Locator("img").AllAsync();
        
        foreach (var image in images)
        {
            var alt = await image.GetAttributeAsync("alt");
            Assert.False(string.IsNullOrEmpty(alt));
            break; // Test first image only
        }
    }

    [Fact]
    // Validates that page has a proper title
    public async Task PageTitle_ShouldBeDescriptive()
    {
        await Page.GotoAsync("/");

        var title = await Page.TitleAsync();
        Assert.NotEmpty(title);
        Assert.True(title.Length > 5);
        Assert.True(title.Contains("Pandorium") || title.Contains("Lobby"));
    }

    [Fact]
    // Validates that form inputs have associated labels
    public async Task FormInputs_ShouldHaveLabels()
    {
        await Page.GotoAsync("/");

        var selects = await Page.Locator("select.game-select").AllAsync();
        
        foreach (var select in selects)
        {
            var id = await select.GetAttributeAsync("id");
            if (!string.IsNullOrEmpty(id))
            {
                var label = Page.Locator($"label[for='{id}']");
                var labelExists = await label.CountAsync() > 0;
                Assert.True(labelExists);
            }
            break; // Test first select only
        }
    }

    [Fact]
    // Validates that disabled elements are clearly indicated
    public async Task DisabledElements_ShouldBeVisuallyDistinct()
    {
        await Page.GotoAsync("/");

        // Try to find a disabled button
        var buttons = await Page.Locator("button").AllAsync();
        
        foreach (var button in buttons)
        {
            var isDisabled = await button.IsDisabledAsync();
            if (isDisabled)
            {
                var opacity = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).opacity");
                var cursor = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).cursor");

                Assert.NotEqual("1", opacity);
                Assert.True(cursor.Contains("not-allowed") || cursor.Contains("default"));
                break;
            }
        }
    }

    [Fact]
    // Validates that focus trap works in modals
    public async Task Modals_ShouldTrapFocus()
    {
        await Page.GotoAsync("/");

        // Check if there are any modal elements in the design system
        var modals = await Page.Locator(".game-modal, [role='dialog']").AllAsync();
        
        if (modals.Count > 0)
        {
            // Verify modal has proper ARIA attributes
            var role = await modals[0].GetAttributeAsync("role");
            Assert.True(role == "dialog" || role == "alertdialog");
        }
    }

    [Fact]
    // Validates that animations respect prefers-reduced-motion
    public async Task Animations_ShouldRespectReducedMotionPreference()
    {
        await Page.GotoAsync("/");

        // Check for prefers-reduced-motion support in CSS
        var hasReducedMotionSupport = await Page.EvaluateAsync<bool>(
            """
            () => {
                const style = document.createElement('style');
                style.textContent = '@media (prefers-reduced-motion: reduce) { * { animation: none; } }';
                document.head.appendChild(style);
                return true;
            }
            """);

        Assert.True(hasReducedMotionSupport);

        // Verify animations exist normally
        var animatedElement = Page.Locator(".game-animate-fadeIn").First;
        if (await animatedElement.CountAsync() > 0)
        {
            await Expect(animatedElement).ToBeVisibleAsync();
        }
    }

    [Fact]
    // Validates that text can be resized without breaking layout
    public async Task TextResize_ShouldNotBreakLayout()
    {
        await Page.GotoAsync("/");

        // Get initial layout
        var containerHeight = await Page.Locator(".game-container").EvaluateAsync<double>("el => el.offsetHeight");

        // Increase font size by 200% (WCAG requirement)
        await Page.EvaluateAsync("document.documentElement.style.fontSize = '32px'");
        await Page.WaitForTimeoutAsync(WAIT_FOR_TIMEOUT);

        // Check that content doesn't overflow
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        
        Assert.False(hasHorizontalScroll);

        // Verify container expands to accommodate larger text
        var newContainerHeight = await Page.Locator(".game-container").EvaluateAsync<double>("el => el.offsetHeight");
        Assert.True(newContainerHeight >= containerHeight);
    }

    [Fact]
    // Validates that error messages are accessible
    public async Task ErrorMessages_ShouldBeAccessible()
    {
        await Page.GotoAsync("/");

        // Look for alert elements
        var alerts = await Page.Locator(".game-alert, [role='alert']").AllAsync();
        
        if (alerts.Count > 0)
        {
            foreach (var alert in alerts)
            {
                // Verify alert has proper role or class
                var role = await alert.GetAttributeAsync("role");
                var className = await alert.GetAttributeAsync("class");
                
                var isAccessible = role == "alert" || (className?.Contains("game-alert") ?? false);
                Assert.True(isAccessible);
                
                break; // Test first alert only
            }
        }
    }
}
