using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.AzureAI's azure-ai-search-default-settings.ts: a small classic
// (non-module) script toggling the API-key vs. managed-identity wrapper based on the
// selected Authentication Type.
public sealed class AzureAiSearchSettingsTests : CmsTestBase<AzureAiSearchSettingsTestsFixture>, IClassFixture<AzureAiSearchSettingsTestsFixture>
{
    public AzureAiSearchSettingsTests(AzureAiSearchSettingsTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ChangingAuthenticationType_TogglesCorrectWrapper()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Settings/azureAISearch");

        var authTypeSelect = page.Locator("select.azure-ai-auth-type");
        var apiKeyWrapper = page.Locator(".azure-ai-api-key-wrapper");
        var identityWrapper = page.Locator(".azure-ai-identity-wrapper");

        await Assertions.Expect(authTypeSelect).ToHaveCountAsync(1);

        // Default: both wrappers hidden.
        await Assertions.Expect(apiKeyWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));
        await Assertions.Expect(identityWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));

        await authTypeSelect.SelectOptionAsync("ApiKey");
        await Assertions.Expect(apiKeyWrapper).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));
        await Assertions.Expect(identityWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));

        await authTypeSelect.SelectOptionAsync("ManagedIdentity");
        await Assertions.Expect(apiKeyWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));
        await Assertions.Expect(identityWrapper).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));

        await authTypeSelect.SelectOptionAsync("Default");
        await Assertions.Expect(apiKeyWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));
        await Assertions.Expect(identityWrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
