using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the editors of the content items added to a BagPart via its AJAX "Add" button
// (src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/flows.edit.ts). Like for the FlowPart, their
// scripts are loaded with the page and only evaluated once, so the editors inserted later must be
// picked up by the scripts themselves. Uses the Agency recipe, whose LandingPage "Portfolio" BagPart
// contains Project items with a MediaField.
public sealed class BagPartEditorTests : CmsTestBase<AgencyFixture>, IClassFixture<AgencyFixture>
{
    public BagPartEditorTests(AgencyFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddItem_ProjectItems_MountMediaFields()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/LandingPage/Create");

        var placeholder = page.Locator(".widget-template-placeholder-bagpart.bagpart-portfolio");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var consoleErrors = page.CollectConsoleErrors();

        var addButton = page.Locator(".add-widget[data-widget-type='Project'][data-part-name='Portfolio']");
        var items = placeholder.Locator(".widget.widget-editor.card[data-content-type='Project']");

        // Two instances, so that the second one can only be mounted by the observer once the
        // media picker module has already handled the first one.
        await addButton.ClickAsync();
        await Assertions.Expect(items).ToHaveCountAsync(1);
        await addButton.ClickAsync();
        await Assertions.Expect(items).ToHaveCountAsync(2);

        for (var i = 0; i < 2; i++)
        {
            var mediaField = items.Nth(i).Locator("[data-media-field-type]");
            await Assertions.Expect(mediaField).ToHaveAttributeAsync("data-media-field-mounted", "true");
            await Assertions.Expect(mediaField.Locator(".mf-toolbar")).ToBeVisibleAsync();
        }

        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }
}
