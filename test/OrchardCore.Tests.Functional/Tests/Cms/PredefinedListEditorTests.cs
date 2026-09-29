using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.ContentFields' TextField PredefinedList options editor
// (options-editor-fields.ts, a Vue 3 options-table-editor.ts consumer) - specifically the
// three requirements from PR #19581's review ("Set the value automatically in Predefined
// List editor"), which this session redid directly against the current Vue 3
// options-table-editor.ts (the PR's own Vue 2 optionsEditor.js no longer exists, having been
// consolidated into the shared component by PR #19489/#19774):
//   1. Once an option's value has been edited directly, changing its label must NOT
//      overwrite that value.
//   2. A brand new option's value is prefilled with its label as soon as the label is typed
//      (no need to type the same text twice).
//   3. Adding a new option with the same label as an existing, currently-default option must
//      NOT silently move the "default" selection onto the new option.
public sealed class PredefinedListEditorTests : CmsTestBase<PredefinedListEditorTestsFixture>, IClassFixture<PredefinedListEditorTestsFixture>
{
    public PredefinedListEditorTests(PredefinedListEditorTestsFixture fixture) : base(fixture) { }

    private static async Task<ILocator> OpenFieldEditorAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/ContentParts/PredefinedListEditorTestPage/Fields/Category/Edit");
        var optionsTable = page.Locator(".options-table");
        await Assertions.Expect(optionsTable).ToHaveCountAsync(1);
        return optionsTable;
    }

    private static ILocator AddRowLink(IPage page) => page.Locator(".options-table-editor-mount a.btn-light");

    [Fact]
    public async Task DirectlyEditedValue_IsNotOverwrittenByLaterLabelEdit()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);

        var row = rows.Nth(0);
        var nameInput = row.Locator("input[type='text']").Nth(0);
        var valueInput = row.Locator("input[type='text']").Nth(1);

        await nameInput.FillAsync("Red");
        // Requirement #2 (auto-fill): typing the label alone mirrors into the still-untouched
        // value column.
        await Assertions.Expect(valueInput).ToHaveValueAsync("Red");

        // Requirement #1: a direct edit to the value column must permanently stop it from
        // being auto-filled by any further label edits.
        await valueInput.FillAsync("custom-red-value");
        await nameInput.FillAsync("Bright Red");
        await Assertions.Expect(valueInput).ToHaveValueAsync("custom-red-value");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task NewOption_ValuePrefillsFromLabel_UntouchedOnlyUntilFirstDirectEdit()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        await AddRowLink(page).ClickAsync();
        var row = rows.Nth(0);
        var nameInput = row.Locator("input[type='text']").Nth(0);
        var valueInput = row.Locator("input[type='text']").Nth(1);

        // Requirement #2, typed incrementally (not a single Fill) - auto-fill must keep
        // mirroring on every keystroke, not just once.
        await nameInput.PressSequentiallyAsync("Blue");
        await Assertions.Expect(valueInput).ToHaveValueAsync("Blue");

        await nameInput.PressSequentiallyAsync(" Sky");
        await Assertions.Expect(valueInput).ToHaveValueAsync("Blue Sky");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task AddingOptionWithSameLabelAsDefault_DoesNotStealTheDefaultSelection()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        // First option: "Green", set as the default.
        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        var firstRow = rows.Nth(0);
        await firstRow.Locator("input[type='text']").Nth(0).FillAsync("Green");

        var firstRadio = firstRow.Locator("input[type='radio']");
        await firstRadio.ClickAsync();
        await Assertions.Expect(firstRadio).ToBeCheckedAsync();

        // Second option, same label ("Green") as the first - because auto-fill mirrors the
        // label into the (still untouched) value column for both rows, they end up with the
        // same value too, which is exactly the scenario requirement #3 guards against: a
        // native-value-keyed radio group would render BOTH as checked, or silently move the
        // selection. The first row's radio must remain the only one checked.
        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        var secondRow = rows.Nth(1);
        await secondRow.Locator("input[type='text']").Nth(0).FillAsync("Green");

        var secondRadio = secondRow.Locator("input[type='radio']");
        await Assertions.Expect(firstRadio).ToBeCheckedAsync();
        await Assertions.Expect(secondRadio).Not.ToBeCheckedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ExistingSavedOption_EditingLabelDoesNotOverwriteItsSavedValue()
    {
        // Regression test for PR #19904 review feedback (gvkries): a row loaded from
        // already-persisted settings has a real value that must never be silently
        // auto-filled over just because its label gets edited - only a BRAND NEW, still-blank
        // row should ever have its value mirrored from the label (requirement #2).
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        // Create one option whose value deliberately differs from its label, and save it -
        // so the next page load reads it back from the persisted settings as a "prefilled" row.
        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        var row = rows.Nth(0);
        var nameInput = row.Locator("input[type='text']").Nth(0);
        var valueInput = row.Locator("input[type='text']").Nth(1);
        await nameInput.FillAsync("Red");
        await valueInput.FillAsync("custom-red-value");

        await page.Locator("button.save[type='submit']").ClickAsync();

        optionsTable = await OpenFieldEditorAsync(page);
        rows = optionsTable.Locator("tbody tr");
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        row = rows.Nth(0);
        nameInput = row.Locator("input[type='text']").Nth(0);
        valueInput = row.Locator("input[type='text']").Nth(1);

        await Assertions.Expect(nameInput).ToHaveValueAsync("Red");
        await Assertions.Expect(valueInput).ToHaveValueAsync("custom-red-value");

        // Editing the label of this freshly-loaded, already-saved row must NOT re-trigger
        // auto-fill: the value must stay exactly what was saved.
        await nameInput.FillAsync("Bright Red");
        await Assertions.Expect(valueInput).ToHaveValueAsync("custom-red-value");

        // Clean up: remove the row and save, so later tests in this fixture-shared page start
        // from an empty options list again.
        await row.Locator("a.btn").Last.ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(0);
        await page.Locator("button.save[type='submit']").ClickAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ValidationError_PreservesInProgressEdits()
    {
        // Regression test for PR #19904 review feedback (gvkries): a validation error (e.g. a
        // duplicate label+value pair) must re-render the admin's in-progress, unsaved edits -
        // not discard them and fall back to whatever was last persisted.
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        var firstRow = rows.Nth(0);
        await firstRow.Locator("input[type='text']").Nth(0).FillAsync("Duplicate");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        var secondRow = rows.Nth(1);
        await secondRow.Locator("input[type='text']").Nth(0).FillAsync("Duplicate");

        // Both rows auto-fill to the same label+value pair - triggers the server-side duplicate
        // validation error on submit. This POSTs back to the SAME edit action, which re-renders
        // the shape driver's UpdateAsync() result directly (no redirect on failure) - so the
        // rows checked below come from that same response, not from re-navigating/reloading.
        await page.Locator("button.save[type='submit']").ClickAsync();

        var optionsTableAfterFailedSubmit = page.Locator(".options-table");
        await Assertions.Expect(optionsTableAfterFailedSubmit).ToHaveCountAsync(1);
        var rowsAfterFailedSubmit = optionsTableAfterFailedSubmit.Locator("tbody tr");

        // The two in-progress "Duplicate" rows must still be there - not reset to the
        // last-saved (empty) state.
        await Assertions.Expect(rowsAfterFailedSubmit).ToHaveCountAsync(2);
        await Assertions.Expect(rowsAfterFailedSubmit.Nth(0).Locator("input[type='text']").Nth(0)).ToHaveValueAsync("Duplicate");
        await Assertions.Expect(rowsAfterFailedSubmit.Nth(1).Locator("input[type='text']").Nth(0)).ToHaveValueAsync("Duplicate");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DraggingARow_ReordersTheOptionsList()
    {
        // Regression test for PR #19904 review feedback (gvkries, second round): dragging a
        // row via its move handle must actually reorder the underlying rows array. The inner
        // <draggable> previously used v-model="rows" against optionsTableComponent's own
        // "rows" PROP (owned by the parent mount, not this component) - vuedraggable replaces
        // the whole array reference on reorder, and Vue silently refuses to write a new
        // reference into a prop, so add/remove (which mutate in place) kept working while
        // drag-reordering was a permanent, silent no-op.
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        await rows.Nth(0).Locator("input[type='text']").Nth(0).FillAsync("First");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        await rows.Nth(1).Locator("input[type='text']").Nth(0).FillAsync("Second");

        var handle0 = rows.Nth(0).Locator(".cursor-move");
        var handle1 = rows.Nth(1).Locator(".cursor-move");
        var box1 = await handle1.BoundingBoxAsync();
        var box0 = await handle0.BoundingBoxAsync();

        // A native HTML5 drag (which SortableJS/vuedraggable use) needs real incremental mouse
        // moves past the drag threshold - a single Playwright DragAndDropAsync jump is often too
        // fast for Sortable to register a valid drag start, so step it manually.
        await page.Mouse.MoveAsync(box1.X + box1.Width / 2, box1.Y + box1.Height / 2);
        await page.Mouse.DownAsync();
        for (var i = 1; i <= 10; i++)
        {
            var y = box1.Y + (box0.Y - box1.Y) * i / 10;
            await page.Mouse.MoveAsync(box1.X + box1.Width / 2, y, new MouseMoveOptions { Steps = 5 });
            await page.WaitForTimeoutAsync(50);
        }
        await page.WaitForTimeoutAsync(200);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(300);

        await Assertions.Expect(rows.Nth(0).Locator("input[type='text']").Nth(0)).ToHaveValueAsync("Second");
        await Assertions.Expect(rows.Nth(1).Locator("input[type='text']").Nth(0)).ToHaveValueAsync("First");

        // Clean up: remove both rows so later tests in this fixture-shared page start clean -
        // no save needed since these edits were never submitted.
        await page.GotoAndAssertOkAsync("/Admin/ContentParts/PredefinedListEditorTestPage/Fields/Category/Edit");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task JsonModal_OpensFromThePencilIcon_AndItsEditsReachTheTable()
    {
        // Regression test for a bug PRE-EXISTING on main since PR #19489's Vue 2 -> Vue 3
        // migration, found while manually testing this PR: clicking the pencil icon did
        // nothing at all. The migration moved the JSON modal's markup out of the Razor
        // <script type="text/x-template"> and into options-table-editor.ts's own component
        // template, but dropped the "<field id>-ModalBody" class that old template carried -
        // while keeping BOTH each consumer entry's
        // document.getElementsByClassName(`${element.id}-ModalBody`) lookup AND
        // initOptionsTableEditor's `if (modalBodyElement)` guard around showModal(). Nothing
        // in the repo rendered that class any more, so the collection was always empty, the
        // guard always false, and the pencil a permanent no-op for all four consumers
        // (MultiTextField, OpenId parameters and Seo custom meta tags too - OpenId's leftover
        // class only survives inside a dead <script type="text/x-template">, whose contents
        // are raw text, not DOM, so getElementsByClassName never saw it either). The guard was
        // vestigial: the modal root is component-owned via ref="modalRoot".
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        var modal = page.Locator(".options-table-editor-mount .modal");
        await Assertions.Expect(modal).Not.ToBeVisibleAsync();

        await page.Locator(".options-table-editor-mount a.float-end").ClickAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();

        await modal.Locator("textarea").FillAsync("""[{"name":"From JSON","value":"json-value"}]""");
        await modal.Locator("button.btn-submit").ClickAsync();
        await Assertions.Expect(modal).Not.ToBeVisibleAsync();

        await Assertions.Expect(rows).ToHaveCountAsync(1);
        var nameInput = rows.Nth(0).Locator("input[type='text']").Nth(0);
        var valueInput = rows.Nth(0).Locator("input[type='text']").Nth(1);
        await Assertions.Expect(nameInput).ToHaveValueAsync("From JSON");
        await Assertions.Expect(valueInput).ToHaveValueAsync("json-value");

        // A row pasted through the modal already carries an explicit value, so
        // markPrefilledRowsAsTouched must treat it as touched: editing its label afterwards
        // must not auto-fill over that value.
        await nameInput.FillAsync("Renamed");
        await Assertions.Expect(valueInput).ToHaveValueAsync("json-value");

        // Clean up: these edits were never submitted, so navigating away is enough to leave
        // later tests in this fixture-shared page a clean options list.
        await page.GotoAndAssertOkAsync("/Admin/ContentParts/PredefinedListEditorTestPage/Fields/Category/Edit");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task JsonModal_DefaultValueInput_SelectsTheMatchingOptionAndPersists()
    {
        // The modal's "Default value" text row was dropped by PR #19489's Vue 2 -> Vue 3
        // migration (the old Razor x-template had it bound to the same `data.selected` as the
        // radio group) and is restored here with the same semantics: typing a value SELECTS
        // the option carrying it, and a value matching no option selects nothing - the radio
        // group remains the only thing that posts DefaultValue.
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        var optionsTable = await OpenFieldEditorAsync(page);
        var rows = optionsTable.Locator("tbody tr");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);
        await rows.Nth(0).Locator("input[type='text']").Nth(0).FillAsync("Red");

        await AddRowLink(page).ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        await rows.Nth(1).Locator("input[type='text']").Nth(0).FillAsync("Blue");

        var modal = page.Locator(".options-table-editor-mount .modal");
        await page.Locator(".options-table-editor-mount a.float-end").ClickAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();

        var defaultValueInput = modal.Locator("input[type='text']");
        await Assertions.Expect(defaultValueInput).ToHaveCountAsync(1);

        // A value matching no option selects nothing - the pre-#19489 behavior, since only a
        // checked radio posts DefaultValue.
        await defaultValueInput.FillAsync("not-an-option");
        await Assertions.Expect(rows.Nth(0).Locator("input[type='radio']")).Not.ToBeCheckedAsync();
        await Assertions.Expect(rows.Nth(1).Locator("input[type='radio']")).Not.ToBeCheckedAsync();

        // Typing an option's value selects that option, exactly as clicking its radio would.
        await defaultValueInput.FillAsync("Blue");
        await Assertions.Expect(rows.Nth(1).Locator("input[type='radio']")).ToBeCheckedAsync();
        await Assertions.Expect(rows.Nth(0).Locator("input[type='radio']")).Not.ToBeCheckedAsync();

        await modal.Locator("button.btn-submit").ClickAsync();
        await Assertions.Expect(modal).Not.ToBeVisibleAsync();
        await page.Locator("button.save[type='submit']").ClickAsync();

        // The selection made by typing must survive the round trip.
        optionsTable = await OpenFieldEditorAsync(page);
        rows = optionsTable.Locator("tbody tr");
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        await Assertions.Expect(rows.Nth(1).Locator("input[type='radio']")).ToBeCheckedAsync();
        await Assertions.Expect(rows.Nth(0).Locator("input[type='radio']")).Not.ToBeCheckedAsync();

        // Clean up: drop both rows and save, so later tests in this fixture-shared page start
        // from an empty options list again.
        await rows.Nth(1).Locator("a.btn").Last.ClickAsync();
        await rows.Nth(0).Locator("a.btn").Last.ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(0);
        await page.Locator("button.save[type='submit']").ClickAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
