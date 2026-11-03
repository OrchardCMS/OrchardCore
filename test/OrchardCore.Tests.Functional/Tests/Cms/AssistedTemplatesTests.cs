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

        // The filter of what the templates render stays, with its value, when a search matches nothing.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Feature?Options.Area=All&Options.Search=NoTemplateMatchesThis");
        await Assertions.Expect(page.Locator("select[name='Options.Area']")).ToHaveValueAsync("All");

        // Shape Tracing inspects the content items and suggests the alternates of the templates.
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Templates.ShapeTracing");

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Templates");
        await Assertions.Expect(InspectContentLink(page)).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateSources/Theme");
        await Assertions.Expect(AlternatesLink(page).First).ToBeVisibleAsync();

        // The content items are picked with the content picker, searching them by their display text.
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateInspector");
        await Assertions.Expect(page.Locator("#TemplateInspector_ContentItem .multiselect")).ToBeVisibleAsync();

        var search = await page.APIRequest.GetAsync($"{prefix}/Admin/TemplateInspector/SearchContentItems?adminTemplates=false&query=About");
        Assert.True(search.Ok);
        var items = (await search.JsonAsync())?.EnumerateArray().Select(item => item.GetProperty("displayText").GetString()).ToList();
        Assert.NotNull(items);
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Contains("About", item, StringComparison.OrdinalIgnoreCase));

        // The front end renders the content item with the site theme, whose template for an article doesn't render its title
        // part, and what this template adds to the layout, e.g. the title of the page, stays out of the inspection page.
        var contentItemId = (await search.JsonAsync())?.EnumerateArray().First().GetProperty("id").GetString();
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/TemplateInspector?contentItemId={contentItemId}");
        var shapes = page.Locator("ul.list-group code.fs-6");
        await Assertions.Expect(shapes.GetByText("HtmlBodyPart", new() { Exact = true })).ToHaveCountAsync(1);
        await Assertions.Expect(shapes.GetByText("TitlePart", new() { Exact = true })).ToHaveCountAsync(0);
        Assert.DoesNotContain("About", await page.TitleAsync(), StringComparison.Ordinal);

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
