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

        // Retry the reload itself, not just the locator assertion: on a distributed
        // cache backend (Redis), the POST /Save response already completed the write
        // AND its own cache invalidation before returning 200 (DocumentManager commits
        // synchronously on shell-scope dispose, before the HTTP response is sent), but
        // this reload's own GET /Admin/DataLocalization/Index runs in a brand new shell
        // scope that reads via GetOrCreateImmutableAsync - which serves from Redis. A
        // Playwright locator assertion only retries reading the CURRENT page's DOM; it
        // can't recover from a page that was rendered server-side with stale data in
        // the first place. Observed live on CI (Redis + Azurite backend only, never
        // Sqlite/Postgres/MySql/SqlServer): "But was: ''" - the reloaded page's input
        // was rendered empty, meaning the server itself briefly served the pre-save
        // value across a page load that started right after the save request settled.
        var editedInput = editedRow.Locator("input[type='text']");
        const int maxReloadAttempts = 5;

        for (var attempt = 1; attempt <= maxReloadAttempts; attempt++)
        {
            var currentValue = await editedInput.InputValueAsync();

            if (currentValue == "Test Translated Value")
            {
                break;
            }

            if (attempt == maxReloadAttempts)
            {
                await Assertions.Expect(editedInput).ToHaveValueAsync("Test Translated Value");
                break;
            }

            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
