using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.DataLocalization's translation-editor.ts, a net-new (not migrated)
// interactive Vue 3 editor with search/filter/auto-save added as part of this Vue2-to-Vue3
// migration effort - 285 lines, never previously covered by any functional test.
//
// Also guards against a real crash bug found while writing this test: OrchardCore.Contents'
// DataLocalizationStartup used to unconditionally register
// ContentTypesAdminNodeDataLocalizationProvider, which requires IAdminMenuAccessor - a
// service only registered when OrchardCore.AdminMenu is enabled, with no feature dependency
// declared anywhere. Enabling OrchardCore.Contents + OrchardCore.DataLocalization without
// OrchardCore.AdminMenu threw an unhandled DI resolution exception (500) on every
// /Admin/DataLocalization/Index request. Fixed by splitting that provider's registration
// into its own [RequireFeatures("OrchardCore.DataLocalization", "OrchardCore.AdminMenu")]
// startup class (see Contents/Startup.cs) - this test's recipe deliberately does NOT enable
// OrchardCore.AdminMenu, so it directly exercises the now-fixed code path.
public sealed class DataLocalizationTests : CmsTestBase<DataLocalizationTestsFixture>, IClassFixture<DataLocalizationTestsFixture>
{
    public DataLocalizationTests(DataLocalizationTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task EditTranslation_Save_PersistsOnReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/DataLocalization/Index");

        var editor = page.Locator("#translation-editor");
        await Assertions.Expect(editor).ToHaveCountAsync(1);

        // Don't depend on a specific translatable string existing - just use whichever
        // row renders first. Enabling OrchardCore.Roles/Contents/ContentTypes (implicit
        // via the recipe's own admin-user/content-type setup) guarantees at least one
        // ILocalizationDataProvider yields translatable strings on a stock setup.
        var visibleRows = editor.Locator("table tbody tr");
        await Assertions.Expect(visibleRows).Not.ToHaveCountAsync(0);

        var firstInput = visibleRows.First.Locator("input[type='text']");
        var originalKey = (await visibleRows.First.Locator("code").TextContentAsync())?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(originalKey), "Expected the first translatable row to expose a non-empty key.");
        await firstInput.FillAsync("");
        await firstInput.FillAsync("Test Translated Value");

        var saveButton = editor.Locator("button.save");
        await Assertions.Expect(saveButton).Not.ToBeDisabledAsync();
        var saveResponseTask = page.WaitForResponseAsync(resp => resp.Url.Contains("/Admin/DataLocalization/Save") && resp.Request.Method == "POST");
        await saveButton.ClickAsync();
        var saveResponse = await saveResponseTask;
        if (!saveResponse.Ok)
        {
            throw new Exception($"Save request failed: {saveResponse.Status} {await saveResponse.TextAsync()}");
        }

        // The save button re-disables once the fetch POST resolves and isDirty resets -
        // waiting for that is the real signal the save round-trip completed, not just a
        // fixed timeout.
        await Assertions.Expect(saveButton).ToBeDisabledAsync();

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Locate the edited row by its exact key text via Playwright's own client-side
        // filtering rather than typing into the app's #search-box: that couples this
        // persistence assertion to the app's search feature working correctly, and a
        // silent search-filter mismatch (e.g. no rows matching) leaves the locator free
        // to resolve to a completely unrelated element later in the DOM - which is
        // exactly what produced a confusing failure here previously.
        var editedRow = page.Locator("#translation-editor table tbody tr")
            .Filter(new LocatorFilterOptions { HasText = originalKey });
        await Assertions.Expect(editedRow).ToHaveCountAsync(1);
        await Assertions.Expect(editedRow.Locator("input[type='text']")).ToHaveValueAsync("Test Translated Value");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
