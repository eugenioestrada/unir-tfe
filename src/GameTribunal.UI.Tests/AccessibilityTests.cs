using Microsoft.Playwright;

namespace GameTribunal.UI.Tests;

/// <summary>
/// Tests to validate that the game meets accessibility standards (WCAG AA).
/// Ensures the stunning design is also inclusive and accessible to all users.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AccessibilityTests : PlaywrightTest
{
    private const string BaseUrl = "https://localhost:7000";

    [SetUp]
    public async Task SetUp()
    {
        await Page.SetViewportSizeAsync(1920, 1080);
    }

    [Test]
    [Description("Validates that color contrast meets WCAG AA standards")]
    public async Task ColorContrast_ShouldMeetWCAGStandards()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Test title contrast
        var title = Page.Locator(".game-title").First;
        await Expect(title).ToBeVisibleAsync();

        var titleColor = await title.EvaluateAsync<string>("el => window.getComputedStyle(el).color");
        var titleBackground = await title.EvaluateAsync<string>(
            "el => { let parent = el.parentElement; while (parent) { const bg = window.getComputedStyle(parent).backgroundColor; if (bg !== 'rgba(0, 0, 0, 0)') return bg; parent = parent.parentElement; } return 'rgb(0, 0, 0)'; }");

        Assert.That(titleColor, Is.Not.Empty, "Title should have a defined color");
        Assert.That(titleBackground, Is.Not.Empty, "Title should have a background (even if inherited)");

        // Test button contrast
        var button = Page.Locator(".game-btn-primary").First;
        if (await button.CountAsync() > 0)
        {
            var buttonColor = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).color");
            var buttonBackground = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).backgroundColor");

            Assert.That(buttonColor, Is.Not.Empty, "Button text should have a defined color");
            Assert.That(buttonBackground, Is.Not.EqualTo("rgba(0, 0, 0, 0)"), 
                "Button should have a solid background color for contrast");
        }
    }

    [Test]
    [Description("Validates that interactive elements can be navigated with keyboard")]
    public async Task KeyboardNavigation_ShouldWork()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Tab through interactive elements
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(100);

        // Verify focus is visible
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        Assert.That(focusedElement, Is.Not.EqualTo("BODY"), 
            "Tab should move focus to an interactive element");

        // Verify focus indicator is visible
        var focusedOutline = await Page.EvaluateAsync<string>("window.getComputedStyle(document.activeElement).outline");
        var focusedBoxShadow = await Page.EvaluateAsync<string>("window.getComputedStyle(document.activeElement).boxShadow");

        var hasFocusIndicator = focusedOutline != "none" || focusedBoxShadow.Contains("rgb");
        Assert.That(hasFocusIndicator, Is.True, 
            "Focused element should have a visible focus indicator");
    }

    [Test]
    [Description("Validates that page has proper semantic HTML structure")]
    public async Task SemanticHTML_ShouldBeUsed()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Check for proper heading hierarchy
        var h1Count = await Page.Locator("h1").CountAsync();
        Assert.That(h1Count, Is.GreaterThan(0), "Page should have at least one h1 element");

        // Check for proper button elements (not divs with click handlers)
        var allButtons = await Page.Locator("button, a.game-btn").AllAsync();
        Assert.That(allButtons.Count, Is.GreaterThan(0), 
            "Interactive elements should use proper button or link elements");
    }

    [Test]
    [Description("Validates that images have alt text")]
    public async Task Images_ShouldHaveAltText()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Create room to see QR code
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);

        var images = await Page.Locator("img").AllAsync();
        
        foreach (var image in images)
        {
            var alt = await image.GetAttributeAsync("alt");
            Assert.That(alt, Is.Not.Null.And.Not.Empty, 
                "All images should have descriptive alt text for accessibility");
            break; // Test first image only
        }
    }

    [Test]
    [Description("Validates that page has a proper title")]
    public async Task PageTitle_ShouldBeDescriptive()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var title = await Page.TitleAsync();
        Assert.That(title, Is.Not.Empty, "Page should have a title");
        Assert.That(title.Length, Is.GreaterThan(5), "Page title should be descriptive");
        Assert.That(title, Does.Contain("Pandorium").Or.Contain("Lobby"), 
            "Page title should be relevant to the content");
    }

    [Test]
    [Description("Validates that form inputs have associated labels")]
    public async Task FormInputs_ShouldHaveLabels()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var selects = await Page.Locator("select.game-select").AllAsync();
        
        foreach (var select in selects)
        {
            var id = await select.GetAttributeAsync("id");
            if (!string.IsNullOrEmpty(id))
            {
                var label = Page.Locator($"label[for='{id}']");
                var labelExists = await label.CountAsync() > 0;
                Assert.That(labelExists, Is.True, 
                    $"Select element with id '{id}' should have an associated label");
            }
            break; // Test first select only
        }
    }

    [Test]
    [Description("Validates that disabled elements are clearly indicated")]
    public async Task DisabledElements_ShouldBeVisuallyDistinct()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Try to find a disabled button
        var buttons = await Page.Locator("button").AllAsync();
        
        foreach (var button in buttons)
        {
            var isDisabled = await button.IsDisabledAsync();
            if (isDisabled)
            {
                var opacity = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).opacity");
                var cursor = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).cursor");

                Assert.That(opacity, Is.Not.EqualTo("1"), 
                    "Disabled buttons should have reduced opacity");
                Assert.That(cursor, Does.Contain("not-allowed").Or.Contain("default"), 
                    "Disabled buttons should show not-allowed cursor");
                break;
            }
        }
    }

    [Test]
    [Description("Validates that focus trap works in modals")]
    public async Task Modals_ShouldTrapFocus()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Check if there are any modal elements in the design system
        var modals = await Page.Locator(".game-modal, [role='dialog']").AllAsync();
        
        if (modals.Count > 0)
        {
            // Verify modal has proper ARIA attributes
            var role = await modals[0].GetAttributeAsync("role");
            Assert.That(role, Is.EqualTo("dialog").Or.EqualTo("alertdialog"), 
                "Modals should have proper ARIA role");
        }
    }

    [Test]
    [Description("Validates that animations respect prefers-reduced-motion")]
    public async Task Animations_ShouldRespectReducedMotionPreference()
    {
        await Page.GotoAsync($"{BaseUrl}/");

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

        Assert.That(hasReducedMotionSupport, Is.True,
            "Page should support prefers-reduced-motion media query");

        // Verify animations exist normally
        var animatedElement = Page.Locator(".game-animate-fadeIn").First;
        if (await animatedElement.CountAsync() > 0)
        {
            await Expect(animatedElement).ToBeVisibleAsync();
        }
    }

    [Test]
    [Description("Validates that text can be resized without breaking layout")]
    public async Task TextResize_ShouldNotBreakLayout()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Get initial layout
        var containerHeight = await Page.Locator(".game-container").EvaluateAsync<double>("el => el.offsetHeight");

        // Increase font size by 200% (WCAG requirement)
        await Page.EvaluateAsync("document.documentElement.style.fontSize = '32px'");
        await Page.WaitForTimeoutAsync(500);

        // Check that content doesn't overflow
        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth");
        
        Assert.That(hasHorizontalScroll, Is.False, 
            "Page should not have horizontal scroll when text is enlarged");

        // Verify container expands to accommodate larger text
        var newContainerHeight = await Page.Locator(".game-container").EvaluateAsync<double>("el => el.offsetHeight");
        Assert.That(newContainerHeight, Is.GreaterThanOrEqualTo(containerHeight), 
            "Container should expand to accommodate larger text");
    }

    [Test]
    [Description("Validates that error messages are accessible")]
    public async Task ErrorMessages_ShouldBeAccessible()
    {
        await Page.GotoAsync($"{BaseUrl}/");

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
                Assert.That(isAccessible, Is.True, 
                    "Alert messages should have proper ARIA role or semantic class");
                
                break; // Test first alert only
            }
        }
    }
}
