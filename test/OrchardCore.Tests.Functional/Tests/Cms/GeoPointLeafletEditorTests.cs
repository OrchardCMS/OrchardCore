using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the Leaflet GeoPointField editor (coordinate-leaflet-field.ts). The same editor can be
// rendered more than once on a page, including later on when adding an item to a BagPart via AJAX,
// so each map must be initialized and only drive its own latitude/longitude inputs. Uses the
// PartToggleEditorTests recipe (test/OrchardCore.Tests.Functional/Fixtures/
// part-toggle-editor-tests.recipe.json), which no shipped recipe covers.
public sealed class GeoPointLeafletEditorTests : CmsTestBase<PartToggleEditorTestsFixture>, IClassFixture<PartToggleEditorTestsFixture>
{
    private const string ContainerScope = ":not(.widget-template *)";

    public GeoPointLeafletEditorTests(PartToggleEditorTestsFixture fixture) : base(fixture) { }

    private static async Task ClickMapAsync(ILocator editor)
    {
        // Away from the zoom controls (top left), the marker (center) and the attribution (bottom right).
        await editor.Locator("[data-leaflet-map]").ClickAsync(new LocatorClickOptions
        {
            Position = new Position { X = 80, Y = 150 },
        });
    }

    [Fact]
    public async Task LeafletEditor_ContainerAndBagItems_InitializeAndUpdateTheirOwnInputs()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/ToggleContainer/Create");

        var containerEditor = page.Locator($"[data-leaflet-editor]{ContainerScope}");
        await Assertions.Expect(containerEditor).ToHaveCountAsync(1);
        await Assertions.Expect(containerEditor.Locator("[data-leaflet-map]")).ToHaveClassAsync(new Regex("leaflet-container"));

        var items = page.Locator(".widget-template-placeholder-bagpart .widget.widget-editor.card[data-content-type='ToggleItem']");
        for (var i = 1; i <= 2; i++)
        {
            await page.Locator(".add-widget[data-widget-type='ToggleItem']").ClickAsync();
            await Assertions.Expect(items).ToHaveCountAsync(i);
        }

        for (var i = 0; i < 2; i++)
        {
            await Assertions.Expect(items.Nth(i).Locator("[data-leaflet-map]")).ToHaveClassAsync(new Regex("leaflet-container"));
        }

        var firstItemEditor = items.First.Locator("[data-leaflet-editor]");
        await ClickMapAsync(firstItemEditor);

        await Assertions.Expect(firstItemEditor.Locator("[data-latitude]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(firstItemEditor.Locator("[data-longitude]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(items.Nth(1).Locator("[data-latitude]")).ToHaveValueAsync("");
        await Assertions.Expect(containerEditor.Locator("[data-latitude]")).ToHaveValueAsync("");

        await ClickMapAsync(containerEditor);
        await Assertions.Expect(containerEditor.Locator("[data-latitude]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(items.Nth(1).Locator("[data-latitude]")).ToHaveValueAsync("");

        await page.CloseAsync();
    }
}
