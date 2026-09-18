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
}
