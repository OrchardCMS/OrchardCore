using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the show/hide toggles of the AutoroutePart ("Disabled") and SitemapPart ("Override sitemap
// configuration") editors (autoroute-part.ts, sitemap-part.ts). Their elements' ids are prefixed by
// the editor, and the same editors can be rendered more than once on a page, including later on when
// adding an item to a BagPart via AJAX, so each toggle must only drive its own editor. Uses the
// PartToggleEditorTests recipe (test/OrchardCore.Tests.Functional/Fixtures/
// part-toggle-editor-tests.recipe.json), which no shipped recipe covers.
public sealed class PartToggleEditorTests : CmsTestBase<PartToggleEditorTestsFixture>, IClassFixture<PartToggleEditorTestsFixture>
{
    private const string ContainerScope = ":not(.widget-template *)";

    public PartToggleEditorTests(PartToggleEditorTestsFixture fixture) : base(fixture) { }

    private static async Task<ILocator> AddItemAsync(IPage page)
    {
        var items = page.Locator(".widget-template-placeholder-bagpart .widget.widget-editor.card[data-content-type='ToggleItem']");

        await page.Locator(".add-widget[data-widget-type='ToggleItem']").ClickAsync();
        await Assertions.Expect(items).ToHaveCountAsync(1);

        return items.First;
    }

    [Fact]
    public async Task AutoroutePartDisabled_ContainerAndBagItem_ToggleTheirOwnEditor()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/ToggleContainer/Create");

        var containerToggle = page.Locator($"[data-autoroute-disabled-toggle]{ContainerScope}");
        var containerSection = page.Locator($"[data-autoroute-disabled-for]{ContainerScope}");
        await Assertions.Expect(containerSection).ToBeVisibleAsync();

        await containerToggle.CheckAsync();
        await Assertions.Expect(containerSection).ToBeHiddenAsync();

        await containerToggle.UncheckAsync();
        await Assertions.Expect(containerSection).ToBeVisibleAsync();

        var item = await AddItemAsync(page);
        var itemSection = item.Locator("[data-autoroute-disabled-for]");
        await Assertions.Expect(itemSection).ToBeVisibleAsync();

        await item.Locator("[data-autoroute-disabled-toggle]").CheckAsync();
        await Assertions.Expect(itemSection).ToBeHiddenAsync();
        await Assertions.Expect(containerSection).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task SitemapPartOverride_ContainerAndBagItem_ToggleTheirOwnEditor()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/ToggleContainer/Create");

        // The BagPart add button is in the default tab, the SitemapPart editors in the "SEO" tab.
        var item = await AddItemAsync(page);
        await item.Locator("[role='tab']").Filter(new LocatorFilterOptions { HasText = "SEO" }).ClickAsync();

        var itemSections = item.Locator("[data-sitemap-override-for]");
        await Assertions.Expect(itemSections).ToHaveCountAsync(3);
        await Assertions.Expect(itemSections.First).ToBeHiddenAsync();

        await item.Locator("[data-sitemap-override-toggle]").CheckAsync();
        for (var i = 0; i < 3; i++)
        {
            await Assertions.Expect(itemSections.Nth(i)).ToBeVisibleAsync();
        }

        await page.Locator($"[role='tab']{ContainerScope}").Filter(new LocatorFilterOptions { HasText = "SEO" }).ClickAsync();

        var containerToggle = page.Locator($"[data-sitemap-override-toggle]{ContainerScope}");
        var containerSections = page.Locator($"[data-sitemap-override-for]{ContainerScope}");
        await Assertions.Expect(containerSections).ToHaveCountAsync(3);
        await Assertions.Expect(containerToggle).ToBeVisibleAsync();
        await Assertions.Expect(containerSections.First).ToBeHiddenAsync();

        await containerToggle.CheckAsync();
        for (var i = 0; i < 3; i++)
        {
            await Assertions.Expect(containerSections.Nth(i)).ToBeVisibleAsync();
        }

        await containerToggle.UncheckAsync();
        await Assertions.Expect(containerSections.First).ToBeHiddenAsync();
        await Assertions.Expect(item.Locator("[data-sitemap-override-toggle]")).ToBeCheckedAsync();

        await page.CloseAsync();
    }
}
