using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AssistedTemplatesTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public AssistedTemplatesTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    [Fact]
    public async Task AssistedTemplates_Enabled_AddsTemplateSourcesAndInspector()
    {
        var page = await Fixture.CreatePageAsync();
        var prefix = $"/{Tenant.Prefix}";
        await AuthHelper.LoginAsync(page, prefix);

        // Opt-in: the templates list only offers to add a template, and the pages of the feature don't exist.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(page.Locator("a.create")).ToBeVisibleAsync();
        await Assertions.Expect(InspectContentLink(page)).ToHaveCountAsync(0);
        await Assertions.Expect(ThemeTemplatesLink(page)).ToHaveCountAsync(0);

        var response = await page.GotoAsync($"{prefix}/Admin/TemplateSources/Theme");
        Assert.Equal(404, response?.Status);

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Contents/ContentItems");
        await Assertions.Expect(page.Locator("a.dropdown-item", new() { HasText = "Inspect templates" })).ToHaveCountAsync(0);

        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates.Assisted");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(InspectContentLink(page)).ToBeVisibleAsync();
        await Assertions.Expect(ThemeTemplatesLink(page)).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Theme");
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateInspector");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Contents/ContentItems");
        await Assertions.Expect(page.Locator("a.dropdown-item", new() { HasText = "Inspect templates" }).First).ToBeAttachedAsync();

        await page.CloseAsync();
    }

    private static ILocator InspectContentLink(IPage page)
        => page.GetByRole(AriaRole.Button, new() { Name = "Inspect Content", Exact = true });

    private static ILocator ThemeTemplatesLink(IPage page)
        => page.GetByRole(AriaRole.Button, new() { Name = "Theme Templates", Exact = true });
}
