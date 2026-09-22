using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class LocalizationTests : CmsTestBase<LocalizationFixture>, IClassFixture<LocalizationFixture>
{
    public LocalizationTests(LocalizationFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AdminRequest_CultureNotInSetupWizardCultureList_UsesTenantConfiguredCulture()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // "sl-SI" is deliberately not part of OrchardCore.Setup's hard-coded setup-wizard culture
        // list, only of this tenant's own Localization settings (configured via the recipe's
        // "settings" step). If OrchardCore.Setup's Startup still re-applies its own culture list
        // after the tenant is initialized, RequestLocalizationOptions loses "sl-SI" and this
        // request silently falls back to the default culture instead.
        await page.GotoAndAssertOkAsync("/Admin?culture=sl-SI&ui-culture=sl-SI");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "sl-SI");

        await page.CloseAsync();
    }
}
