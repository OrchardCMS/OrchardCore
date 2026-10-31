using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AssistedTemplatesWithoutContentsTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public AssistedTemplatesWithoutContentsTests(CmsSetupFixture fixture) : base(fixture) { }

    // The SaaS recipe doesn't enable the Contents feature.
    protected override string RecipeName => "SaaS";

    [Fact]
    public async Task AssistedTemplates_ContentsDisabled_ListsTemplateSources()
    {
        var page = await Fixture.CreatePageAsync();
        var prefix = $"/{Tenant.Prefix}";
        await AuthHelper.LoginAsync(page, prefix);

        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates");
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates.Assisted");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Features");
        await Assertions.Expect(page.Locator("#btn-enable-OrchardCore_Contents")).ToHaveCountAsync(1);

        // The template sources don't need the content services, which only the Shape Tracing feature uses.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Theme");
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Feature");
        await Assertions.Expect(page.Locator("a.btn", new() { HasText = "Alternates" })).ToHaveCountAsync(0);

        var response = await page.GotoAsync($"{prefix}/Admin/TemplateInspector");
        Assert.Equal(404, response?.Status);

        await page.CloseAsync();
    }
}
