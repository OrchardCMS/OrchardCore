using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AssistedTemplatesTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public AssistedTemplatesTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    [Fact]
    public async Task AssistedTemplates_Enabled_AddsTemplateSourcesThenShapeTracingAddsInspector()
    {
        var page = await Fixture.CreatePageAsync();
        var prefix = $"/{Tenant.Prefix}";
        await AuthHelper.LoginAsync(page, prefix);

        // Opt-in: the templates list only offers to add a template, and the pages of the features don't exist.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(page.Locator("a.create")).ToBeVisibleAsync();
        await Assertions.Expect(InspectContentLink(page)).ToHaveCountAsync(0);
        await Assertions.Expect(ThemeTemplatesLink(page)).ToHaveCountAsync(0);

        var response = await page.GotoAsync($"{prefix}/Admin/TemplateSources/Theme");
        Assert.Equal(404, response?.Status);

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Contents/ContentItems");
        await Assertions.Expect(InspectTemplatesAction(page)).ToHaveCountAsync(0);

        // Assisted Templates lists the template sources, without inspecting the content items.
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates.Assisted");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(ThemeTemplatesLink(page)).ToBeVisibleAsync();
        await Assertions.Expect(InspectContentLink(page)).ToHaveCountAsync(0);

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Theme");
        await Assertions.Expect(AlternatesLink(page)).ToHaveCountAsync(0);

        response = await page.GotoAsync($"{prefix}/Admin/TemplateInspector");
        Assert.Equal(404, response?.Status);

        // Shape Tracing inspects the content items and suggests the alternates of the templates.
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates.ShapeTracing");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(InspectContentLink(page)).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Theme");
        await Assertions.Expect(AlternatesLink(page).First).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateInspector");

        // The search filters the content items of the picker by their display text.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateInspector?search=About");
        var pickerItems = await page.Locator("select[name='contentItemId'] option:not([value=''])").AllInnerTextsAsync();
        Assert.NotEmpty(pickerItems);
        Assert.All(pickerItems, item => Assert.Contains("About", item, StringComparison.OrdinalIgnoreCase));

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Contents/ContentItems");
        await Assertions.Expect(InspectTemplatesAction(page).First).ToBeAttachedAsync();

        await page.CloseAsync();
    }

    private static ILocator InspectContentLink(IPage page)
        => page.GetByRole(AriaRole.Button, new() { Name = "Inspect Content", Exact = true });

    private static ILocator ThemeTemplatesLink(IPage page)
        => page.GetByRole(AriaRole.Button, new() { Name = "Theme Templates", Exact = true });

    private static ILocator AlternatesLink(IPage page)
        => page.Locator("a.btn", new() { HasText = "Alternates" });

    private static ILocator InspectTemplatesAction(IPage page)
        => page.Locator("a.dropdown-item", new() { HasText = "Inspect templates" });
}
