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
        page.PageError += (_, error) => consoleErrors.Add(error);

        await page.GotoAndAssertOkAsync("/Admin/DataLocalization/Index");

        var editor = page.Locator("#translation-editor");
        await Assertions.Expect(editor).ToHaveCountAsync(1);

        // Don't depend on a specific translatable string existing - just use whichever
        // row renders first. Enabling OrchardCore.Roles/Contents/ContentTypes (implicit
        // via the recipe's own admin-user/content-type setup) guarantees at least one
        // ILocalizationDataProvider yields translatable strings on a stock setup.
        var visibleRows = editor.Locator("table tbody tr");
        Assert.Empty(consoleErrors);
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

    [Fact]
    public async Task UiOverrides_SharedEditor_AutosaveFiltersCultureImportAndRemoval()
    {
        var page = await Fixture.CreatePageAsync();
        page.SetDefaultNavigationTimeout(120000);
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();
        page.PageError += (_, error) => consoleErrors.Add(error);
        await page.GotoAndAssertOkAsync("/Admin/Localization/UI/Index");
        Assert.Contains("\"fr\"", await page.Locator("#translation-editor").GetAttributeAsync("data-cultures"), StringComparison.Ordinal);
        await page.GotoAndAssertOkAsync("/Admin/Localization/UI/Index?culture=fr&ui-culture=fr&search=UI%20Translations");
        var editor = page.Locator("#translation-editor");
        await Assertions.Expect(editor).ToHaveAttributeAsync("data-ui-localization", "true");
        await Assertions.Expect(editor.Locator("#auto-save-toggle")).ToBeCheckedAsync();
        await Assertions.Expect(editor.Locator("fieldset.translations-list")).Not.ToHaveCountAsync(0);
        var row = UiRow(page, "OrchardCore.DataLocalization.Views.Admin.Index", "UI Translations");
        await Assertions.Expect(row).ToHaveCountAsync(1);
        const string text = "<img src=x onerror=alert(1)>";
        var responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Save") && response.Request.Method == "POST");
        await row.Locator("textarea").FillAsync(text);
        var response = await responseTask;
        Assert.True(response.Ok, await response.TextAsync());
        await Assertions.Expect(editor.Locator("button.save")).ToBeDisabledAsync();

        await editor.Locator("#search-box").FillAsync(text);
        await Assertions.Expect(editor.Locator("tbody tr")).ToHaveCountAsync(1);
        await editor.Locator("#missing-only-toggle").CheckAsync();
        await Assertions.Expect(editor.Locator("tbody tr")).ToHaveCountAsync(0);
        await editor.Locator("#missing-only-toggle").UncheckAsync();
        await editor.Locator("#search-box").FillAsync("UI Translations");
        await Assertions.Expect(page.Locator("img[onerror]")).ToHaveCountAsync(0);
        await page.ReloadAsync();
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync(text);
        await Assertions.Expect(page.Locator("h1").First).ToHaveTextAsync(text);
        await Assertions.Expect(page.Locator("img[onerror]")).ToHaveCountAsync(0);

        var downloadTask = page.WaitForDownloadAsync();
        await editor.GetByRole(AriaRole.Link, new() { Name = "Export overrides" }).ClickAsync();
        var download = await downloadTask;
        var export = await File.ReadAllTextAsync(await download.PathAsync(), TestContext.Current.CancellationToken);
        Assert.Contains(text, export, StringComparison.Ordinal);
        responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Save") && response.Request.Method == "POST");
        await row.GetByRole(AriaRole.Button, new() { Name = "Restore fallback" }).ClickAsync();
        Assert.True((await responseTask).Ok);
        await Assertions.Expect(editor.Locator("button.save")).ToBeDisabledAsync();
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync("");

        await editor.Locator("#import-file").SetInputFilesAsync(new FilePayload
        {
            Name = "overrides.po",
            MimeType = "text/plain",
            Buffer = System.Text.Encoding.UTF8.GetBytes(export),
        });
        responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Import") && response.Request.Method == "POST");
        await editor.GetByRole(AriaRole.Button, new() { Name = "Import", Exact = true }).ClickAsync();
        Assert.True((await responseTask).Ok);
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync(text);

        var previousErrors = consoleErrors.Count;
        await page.RouteAsync("**/Localization/UI/GetStrings?culture=fr", route => route.FulfillAsync(new()
        {
            Status = 500,
            ContentType = "application/json",
            Body = """{"message":"Reload unavailable."}""",
        }));
        await editor.GetByRole(AriaRole.Button, new() { Name = "Import", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByText("Reload unavailable.", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(row.Locator("textarea")).ToBeDisabledAsync();
        var reloadErrors = consoleErrors.Skip(previousErrors).ToArray();
        Assert.Contains(reloadErrors, error => error.StartsWith("Load error:", StringComparison.Ordinal));
        Assert.All(reloadErrors, error => Assert.True(error.StartsWith("Load error:", StringComparison.Ordinal) || error.Contains("500", StringComparison.Ordinal)));
        consoleErrors.RemoveRange(previousErrors, consoleErrors.Count - previousErrors);
        await page.UnrouteAsync("**/Localization/UI/GetStrings?culture=fr");
        await page.ReloadAsync();
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync(text);
        await Assertions.Expect(row.Locator("textarea")).Not.ToBeDisabledAsync();

        await editor.Locator("#auto-save-toggle").UncheckAsync();
        var saveStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSave = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.RouteAsync("**/Localization/UI/Save", async route =>
        {
            saveStarted.TrySetResult();
            await releaseSave.Task;
            await route.ContinueAsync();
        });
        try
        {
            await row.Locator("textarea").FillAsync("First update");
            responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Save") && response.Request.Method == "POST");
            await editor.Locator("button.save").ClickAsync();
            await saveStarted.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
            await row.Locator("textarea").FillAsync("Second update");
            releaseSave.TrySetResult();
            Assert.True((await responseTask).Ok);
            await Assertions.Expect(editor.Locator("button.save")).Not.ToBeDisabledAsync();
        }
        finally
        {
            releaseSave.TrySetResult();
            await page.UnrouteAsync("**/Localization/UI/Save");
        }

        responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Save") && response.Request.Method == "POST");
        await editor.Locator("button.save").ClickAsync();
        var updated = await responseTask;
        Assert.True(updated.Ok);
        Assert.Contains("Second update", updated.Request.PostData, StringComparison.Ordinal);
        await Assertions.Expect(editor.Locator("button.save")).ToBeDisabledAsync();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            if (await row.Locator("textarea").InputValueAsync() == "Second update")
            {
                break;
            }
        }

        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync("Second update");
        await editor.Locator("#auto-save-toggle").UncheckAsync();
        await row.Locator("textarea").FillAsync("Unsaved");
        page.Dialog += DismissDialog;
        await editor.Locator("#culture-select").SelectOptionAsync("ar");
        await Assertions.Expect(editor.Locator("#culture-select")).ToHaveValueAsync("fr");
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync("Unsaved");
        page.Dialog -= DismissDialog;
        page.Dialog += AcceptDialog;
        await editor.Locator("#culture-select").SelectOptionAsync("ar");
        await Assertions.Expect(editor.Locator("#culture-select")).ToHaveValueAsync("ar");
        await Assertions.Expect(row.Locator("textarea")).ToHaveValueAsync("");
        page.Dialog -= AcceptDialog;

        // Pick an authoritative plural entry from the initial catalog rather than a hard-coded module string.
        var source = await page.EvaluateAsync<string[]>(
            """
            () => {
                const providers = JSON.parse(document.querySelector('#translation-editor').dataset.providers);
                const entry = providers.flatMap(p => p.subGroups.flatMap(g => g.strings)).find(s => s.plural && s.formatArguments?.length);
                return [entry.context, entry.key];
            }
            """);
        await editor.Locator("#search-box").FillAsync(source[1]);
        var plural = UiRow(page, source[0], source[1]);
        await Assertions.Expect(plural.Locator("textarea")).ToHaveCountAsync(6);
        await Assertions.Expect(plural.GetByText("Available placeholders:")).ToBeVisibleAsync();
        await Assertions.Expect(plural.Locator("label.ocat-label")).ToHaveTextAsync([
            "Translation for count 0", "Translation for count 1", "Translation for count 2",
            "Translation for count 3", "Translation for count 11", "Translation for count 100",
        ]);
        await editor.Locator("#auto-save-toggle").CheckAsync();
        var saves = 0;
        page.Request += CountSaves;
        await plural.Locator("textarea").First.FillAsync("First form");
        await page.WaitForTimeoutAsync(2300);
        Assert.Equal(0, saves);
        responseTask = page.WaitForResponseAsync(response => response.Url.Contains("/Localization/UI/Save") && response.Request.Method == "POST");
        for (var form = 1; form < 6; form++)
        {
            await plural.Locator("textarea").Nth(form).FillAsync($"Form {form}");
        }

        Assert.True((await responseTask).Ok);
        await Assertions.Expect(editor.Locator("button.save")).ToBeDisabledAsync();
        page.Request -= CountSaves;
        Assert.Empty(consoleErrors);
        await page.CloseAsync();

        void CountSaves(object sender, IRequest request)
        {
            if (request.Url.Contains("/Localization/UI/Save") && request.Method == "POST")
            {
                Interlocked.Increment(ref saves);
            }
        }

        async void DismissDialog(object sender, IDialog dialog) => await dialog.DismissAsync();
        async void AcceptDialog(object sender, IDialog dialog) => await dialog.AcceptAsync();
    }

    private static ILocator UiRow(IPage page, string context, string key)
        => page.Locator("#translation-editor .ms-3")
            .Filter(new LocatorFilterOptions { Has = page.Locator("h6").Filter(new LocatorFilterOptions { HasText = context }) })
            .Locator("tbody tr")
            .Filter(new LocatorFilterOptions { Has = page.GetByText(key, new PageGetByTextOptions { Exact = true }) });
}
