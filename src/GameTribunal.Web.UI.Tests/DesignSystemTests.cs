namespace GameTribunal.Web.UI.Tests;

/// <summary>
/// Tests to validate the game's design system meets the highest standards.
/// These tests ensure visual consistency, accessibility, and stunning aesthetics.
/// </summary>
public class DesignSystemTests : PlaywrightTest
{
    private const string BaseUrl = "https://localhost:7000";

            // Configure viewport for desktop testing
            [Fact]
    // Validates that the hero section has impressive visual effects
    public async Task HeroSection_ShouldHaveStunningVisualEffects()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Verify hero section exists and is visible
        var heroSection = Page.Locator(".game-hero");
        await Expect(heroSection).ToBeVisibleAsync();

        // Check for gradient background
        var backgroundStyle = await WaitForComputedStyleAsync(heroSection, "background");
        Assert.Contains("gradient", backgroundStyle).Or.Contain("rgba"), 
            "Hero section should have a gradient or transparent background for visual depth");

        // Verify backdrop-filter blur for glassmorphism effect
        var backdropFilter = await WaitForComputedStyleAsync(heroSection, "backdropFilter");
        Assert.Contains("blur", backdropFilter), 
            "Hero section should have backdrop-filter blur for modern glassmorphism effect");

        // Check for border-radius for rounded corners
        var borderRadius = await WaitForComputedStyleAsync(heroSection, "borderRadius");
        Assert.NotEqual("0px", borderRadius), 
            "Hero section should have rounded corners for a modern look");

        // Verify box-shadow for depth
        var boxShadow = await WaitForComputedStyleAsync(heroSection, "boxShadow");
        Assert.NotEqual("none", boxShadow), 
            "Hero section should have a box-shadow for visual depth");
    }

    [Fact]
    // Validates that buttons have engaging hover effects and animations
    public async Task Buttons_ShouldHaveEngagingHoverEffects()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var primaryButton = Page.Locator(".game-btn-primary").First;
        await Expect(primaryButton).ToBeVisibleAsync();

        // Get initial transform
        var initialTransform = await WaitForComputedStyleAsync(primaryButton, "transform");

        // Hover over the button
        await primaryButton.HoverAsync();

        // Wait a bit for animation
        await Page.WaitForTimeoutAsync(300);

        // Get transform after hover
        var hoverTransform = await WaitForComputedStyleAsync(primaryButton, "transform");

        // Verify that transform changed (button should lift or scale on hover)
        Assert.NotEqual(initialTransform, hoverTransform), 
            "Button should have a transform effect on hover (lift or scale)");

        // Verify transition property exists
        var transition = await WaitForComputedStyleAsync(primaryButton, "transition");
        Assert.NotEqual("all 0s ease 0s", transition), 
            "Button should have smooth transitions");

        // Check for box-shadow enhancement on hover
        var boxShadow = await WaitForComputedStyleAsync(primaryButton, "boxShadow");
        Assert.NotEqual("none", boxShadow), 
            "Button should have an enhanced box-shadow on hover");
    }

    [Fact]
    // Validates that cards have professional styling with depth and shadows
    public async Task Cards_ShouldHaveProfessionalStyling()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var gameCard = Page.Locator(".game-card").First;
        await Expect(gameCard).ToBeVisibleAsync();

        // Verify backdrop-filter for glassmorphism
        var backdropFilter = await WaitForComputedStyleAsync(gameCard, "backdropFilter");
        Assert.Contains("blur", backdropFilter), 
            "Cards should have backdrop-filter blur for glassmorphism effect");

        // Verify border-radius
        var borderRadius = await WaitForComputedStyleAsync(gameCard, "borderRadius");
        var radiusValue = double.Parse(borderRadius.Replace("px", ""));
        Assert.True(radiusValue >= 12), 
            "Cards should have significant border-radius (>=12px) for modern design");

        // Verify box-shadow for depth
        var boxShadow = await WaitForComputedStyleAsync(gameCard, "boxShadow");
        Assert.NotEqual("none", boxShadow), 
            "Cards should have box-shadow for depth");

        // Verify border for subtle definition
        var border = await WaitForComputedStyleAsync(gameCard, "border");
        Assert.NotEqual("0px none rgb(0, 0, 0, border)"), 
            "Cards should have a subtle border for definition");

        // Check for hover transform effect
        await gameCard.HoverAsync();
        await Page.WaitForTimeoutAsync(300);
        
        var transform = await WaitForComputedStyleAsync(gameCard, "transform");
        Assert.NotEqual("none", transform), 
            "Cards should have a subtle transform effect on hover");
    }

    [Fact]
    // Validates that the color palette is vibrant and eye-catching
    public async Task ColorPalette_ShouldBeVibrantAndEyeCatching()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Check CSS custom properties (design tokens)
        var primaryColor = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).getPropertyValue('--color-primary').trim()");
        
        // TODO: Convert to xUnit - Assert.That(primaryColor, Is.Not.Empty, 
            "Primary color should be defined as a CSS custom property");

        // Verify gradient usage
        var heroBackground = await WaitForComputedStyleAsync(Page.Locator(".game-hero"), "background");
        
        Assert.Contains("gradient", heroBackground).Or.Contain("rgba"), 
            "Hero should use gradients for visual impact");

        // Check for primary button gradient
        var buttonBackground = await WaitForComputedStyleAsync(Page.Locator(".game-btn-primary").First, "background");
        
        Assert.Contains("gradient", buttonBackground).Or.Contain("linear"), 
            "Primary buttons should use gradients for vibrant appearance");
    }

    [Fact]
    // Validates that typography has proper hierarchy and readability
    public async Task Typography_ShouldHaveProperHierarchy()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var title = Page.Locator(".game-title").First;
        await Expect(title).ToBeVisibleAsync();

        // Verify font size is large and impactful
        var fontSize = await WaitForComputedStyleAsync(title, "fontSize");
        var fontSizeValue = double.Parse(fontSize.Replace("px", ""));
        Assert.True(fontSizeValue >= 36), 
            "Main title should have large font size (>=36px) for impact");

        // Verify font weight is bold
        var fontWeight = await WaitForComputedStyleAsync(title, "fontWeight");
        var fontWeightValue = int.Parse(fontWeight);
        Assert.True(fontWeightValue >= 700), 
            "Main title should be bold (font-weight >= 700)");

        // Verify text-shadow for depth
        var textShadow = await WaitForComputedStyleAsync(title, "textShadow");
        Assert.NotEqual("none", textShadow), 
            "Title should have text-shadow for visual depth");

        // Verify letter-spacing for premium feel
        var letterSpacing = await WaitForComputedStyleAsync(title, "letterSpacing");
        Assert.NotEqual("normal", letterSpacing), 
            "Title should have custom letter-spacing for refined typography");
    }

    [Fact]
    // Validates that animations are smooth and add to the user experience
    public async Task Animations_ShouldBeSmoothAndPolished()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Check for CSS animations
        var animatedElements = await Page.Locator(".game-animate-fadeIn, .game-animate-slideIn").AllAsync();
        Assert.True(animatedElements.Count > 0), 
            "Page should have animated elements for dynamic feel");

        // Verify animation properties
        if (animatedElements.Count > 0)
        {
            var animation = await animatedElements[0].EvaluateAsync<string>("el => window.getComputedStyle(el).animation");
            Assert.NotEqual("none", animation), 
                "Animated elements should have animation property defined");
        }

        // Check for smooth transitions
        var allButtons = await Page.Locator(".game-btn").AllAsync();
        if (allButtons.Count > 0)
        {
            var transition = await allButtons[0].EvaluateAsync<string>("el => window.getComputedStyle(el).transition");
            Assert.Contains("cubic-bezier", transition).Or.Contain("ease"), 
                "Buttons should use easing functions for smooth transitions");
        }
    }

    [Fact]
    // Validates that the design uses modern effects like glassmorphism
    public async Task ModernEffects_ShouldBePresent()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Count elements with backdrop-filter (glassmorphism effect)
        var glassmorphElements = await Page.EvaluateAsync<int>(@"
            Array.from(document.querySelectorAll('*')).filter(el => {
                const style = window.getComputedStyle(el);
                return style.backdropFilter && style.backdropFilter !== 'none';
            }).length
        ");

        Assert.True(glassmorphElements > 0), 
            "Design should use glassmorphism (backdrop-filter) for modern aesthetic");

        // Check for elements with gradients
        var gradientElements = await Page.EvaluateAsync<int>(@"
            Array.from(document.querySelectorAll('*')).filter(el => {
                const style = window.getComputedStyle(el);
                return style.background && style.background.includes('gradient');
            }).length
        ");

        Assert.True(gradientElements > 0), 
            "Design should use gradients for vibrant, modern look");
    }

    [Fact]
    // Validates that spacing and layout follow a consistent scale
    public async Task Spacing_ShouldFollowConsistentScale()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Verify spacing scale is defined
        var spaceSmall = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).getPropertyValue('--space-sm').trim()");
        var spaceMedium = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).getPropertyValue('--space-md').trim()");
        var spaceLarge = await Page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).getPropertyValue('--space-lg').trim()");

        Assert.Multiple(() =>
        {
            // TODO: Convert to xUnit - Assert.That(spaceSmall, Is.Not.Empty, "Small spacing token should be defined");
            // TODO: Convert to xUnit - Assert.That(spaceMedium, Is.Not.Empty, "Medium spacing token should be defined");
            // TODO: Convert to xUnit - Assert.That(spaceLarge, Is.Not.Empty, "Large spacing token should be defined");
        });

        // Verify that elements use consistent gap/spacing
        var stacks = await Page.Locator(".game-stack").AllAsync();
        if (stacks.Count > 0)
        {
            var gap = await stacks[0].EvaluateAsync<string>("el => window.getComputedStyle(el).gap");
            Assert.NotEqual("normal", gap), 
                "Stack components should use consistent gap spacing");
        }
    }

    [Fact]
    // Validates that interactive elements have clear focus states for accessibility
    public async Task FocusStates_ShouldBeClearAndAccessible()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        var button = Page.Locator(".game-btn").First;
        await Expect(button).ToBeVisibleAsync();

        // Focus the button
        await button.FocusAsync();
        await Page.WaitForTimeoutAsync(100);

        // Check for outline or box-shadow on focus
        var outline = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).outline");
        var boxShadow = await button.EvaluateAsync<string>("el => window.getComputedStyle(el).boxShadow");

        var hasFocusIndicator = outline != "none" || boxShadow.Contains("rgb");
        // TODO: Convert to xUnit - Assert.That(hasFocusIndicator, Is.True, 
            "Focused elements should have clear visual indicators (outline or box-shadow)");
    }

    [Fact]
    // Validates that the QR code display is visually stunning and prominent
    public async Task QRCodeDisplay_ShouldBeVisuallyStunning()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Create a room to see QR code
        var createButton = Page.Locator("button:has-text('Crear Sala')");
        await createButton.ClickAsync();

        // Wait for room creation
        await Page.WaitForTimeoutAsync(2000);

        var qrContainer = Page.Locator(".game-qr-container");
        if (await qrContainer.CountAsync() > 0)
        {
            await Expect(qrContainer).ToBeVisibleAsync();

            // Verify visual enhancements
            var background = await qrContainer.EvaluateAsync<string>("el => window.getComputedStyle(el).background");
            Assert.Contains("gradient", background).Or.Contain("rgba"), 
                "QR container should have an eye-catching background");

            var border = await qrContainer.EvaluateAsync<string>("el => window.getComputedStyle(el).border");
            Assert.NotEqual("0px none rgb(0, 0, 0, border)"), 
                "QR container should have a decorative border");

            // Check room code styling
            var roomCode = Page.Locator(".game-room-code");
            if (await roomCode.CountAsync() > 0)
            {
                var codeFontSize = await roomCode.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");
                var codeFontSizeValue = double.Parse(codeFontSize.Replace("px", ""));
                Assert.True(codeFontSizeValue >= 36), 
                    "Room code should be large and prominent (>=36px)");

                var codeBoxShadow = await roomCode.EvaluateAsync<string>("el => window.getComputedStyle(el).boxShadow");
                Assert.NotEqual("none", codeBoxShadow), 
                    "Room code should have visual emphasis with box-shadow");
            }
        }
    }
}
