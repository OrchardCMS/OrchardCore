using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

/// <summary>
/// Renders the page of a rate limit policy, which lists the limiter sources, and checks that their names, declared as
/// static <c>LocalizationSource</c> members, use the PO file translations of the tenant in the culture of the request.
/// </summary>
public class RateLimiterSourceLocalizationTests
{
    [Fact]
    public async Task Sources_CultureOfTheRequest_UseTheTranslationsOfTheTenant()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        await TenantLocalizationTestHelper.WritePoFileAsync(
            context,
            "fr",
            ("OrchardCore.RateLimits.Services.FixedWindowRateLimiterSource", "Fixed window", "Fenêtre fixe"),
            ("OrchardCore.RateLimits.Services.TokenBucketRateLimiterSource", "Token bucket", "Seau de jetons"));

        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"], "OrchardCore.RateLimits");

        var policy = new RateLimitPolicy
        {
            PolicyId = Guid.NewGuid().ToString("n"),
            Name = "Localization policy",
            IsEnabled = false,
        };

        await context.UsingTenantScopeAsync(async scope => await scope.ServiceProvider.GetRequiredService<IRateLimitPolicyStore>().CreateAsync(policy));

        // Act
        var french = await TenantLocalizationTestHelper.GetPageAsync(context, $"Admin/RateLimits/Edit/{policy.PolicyId}", "fr");
        var english = await TenantLocalizationTestHelper.GetPageAsync(context, $"Admin/RateLimits/Edit/{policy.PolicyId}", "en");
        var frenchAgain = await TenantLocalizationTestHelper.GetPageAsync(context, $"Admin/RateLimits/Edit/{policy.PolicyId}", "fr");

        // Assert. The same singleton sources render in both cultures, so a translation frozen by the first request fails.
        Assert.Contains("Fenêtre fixe", french.Body.TextContent);
        Assert.Contains("Seau de jetons", french.Body.TextContent);
        Assert.Contains("Fixed window", english.Body.TextContent);
        Assert.DoesNotContain("Fenêtre fixe", english.Body.TextContent);
        Assert.Contains("Fenêtre fixe", frenchAgain.Body.TextContent);
    }
}
