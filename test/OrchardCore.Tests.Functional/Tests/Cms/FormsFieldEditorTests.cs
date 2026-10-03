using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Forms' SelectPart editor (select-part-editor.ts, a Vue 3 draggable
// options table + JSON-edit-modal, migrated from Vue 2 in PR #19774), which is addable
// via the Flows/Bag AJAX "Add Widget" flow - exactly the double-instance-init bug class
// PR #19442 fixed for other views. AJAX-adds two SelectPart widget instances and asserts
// each Vue app instance's state (its options table rows) is genuinely independent, not
// just that a DOM count changed (see FeaturesIndexTests' lessons-learned: a row COUNT
// changing could still happen via a shared/leaked module-level array even if the visual
// effect looks instance-scoped, so this test types distinguishable text into each
// instance's row and asserts it does NOT leak into the other instance).
//
// Writing this test caught a real, independently-shipped regression (see issue #19772):
// select-part-editor.ts (and two shared bloom components with the identical pattern,
// options-table-editor.ts and multiselect-picker.ts) registered vuedraggable's UMD global
// as `draggable: vuedraggable.default`. The vuedraggable@4.1.0 UMD bundle's wrapper ends
// with `})["default"]`, which unwraps webpack's `__webpack_exports__["default"]` one level
// before assigning to `root["vuedraggable"]` - so `window.vuedraggable` IS the component
// itself, not an ES-module-shaped `{ default: ... }` namespace object. Reading
// `.default` off it is `undefined`, which silently registers `draggable` as an unresolved
// Vue component: Vue then renders the literal, non-reactive `<draggable>` HTML tag instead
// of the real one, so no option rows ever render, "Add an option" is a permanent no-op, and
// nothing is draggable - on BOTH a normal full-page load and the AJAX-injection path this
// test exercises (confirmed via a throwaway control test against a non-Flow content type,
// since it's a general regression, not something specific to AJAX injection). Zero
// functional test coverage exercised any vuedraggable-based Vue 3 component before this,
// so it shipped silently across all three consumers. Fixed by using `vuedraggable` directly.
public sealed class FormsFieldEditorTests : CmsTestBase<FormsFieldEditorTestsFixture>, IClassFixture<FormsFieldEditorTestsFixture>
{
    public FormsFieldEditorTests(FormsFieldEditorTestsFixture fixture) : base(fixture) { }

    private static ILocator WidgetOfType(IPage page, string contentType)
        => page.Locator($".widget.widget-editor.card[data-content-type='{contentType}']");

    [Fact]
    public async Task AjaxAddedSelectPartWidgets_OptionRowsInitializeIndependently()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/FormsFieldEditorTestPage/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Forms Field Editor AJAX Test");

        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var addWidgetDropdown = page.Locator(".btn-widget-add-below .dropdown-toggle");
        var widgets = placeholder.Locator(".widget-template");

        await addWidgetDropdown.ClickAsync();
        await page.Locator("a.dropdown-item.add-widget[data-widget-type='FormsFieldEditorTestWidget']").ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(1);

        await addWidgetDropdown.ClickAsync();
        await page.Locator("a.dropdown-item.add-widget[data-widget-type='FormsFieldEditorTestWidget']").ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(2);

        var selectWidgets = WidgetOfType(page, "FormsFieldEditorTestWidget");
        var firstInstance = selectWidgets.Nth(0);
        var secondInstance = selectWidgets.Nth(1);

        // Each SelectPart widget starts with zero option rows - "Add an option" adds one.
        // Wait for the Vue app to actually mount in each AJAX-injected instance (the
        // options table itself only exists once Vue.createApp(...).mount() has run) before
        // interacting with it.
        await Assertions.Expect(firstInstance.Locator(".select-options-table")).ToHaveCountAsync(1);
        await Assertions.Expect(secondInstance.Locator(".select-options-table")).ToHaveCountAsync(1);

        var addOptionLinkFirst = firstInstance.Locator(".select-options-table a.btn-light");
        var addOptionLinkSecond = secondInstance.Locator(".select-options-table a.btn-light");
        var optionRowsFirst = firstInstance.Locator(".select-options-table tbody tr");
        var optionRowsSecond = secondInstance.Locator(".select-options-table tbody tr");

        await Assertions.Expect(optionRowsFirst).ToHaveCountAsync(0);
        await Assertions.Expect(optionRowsSecond).ToHaveCountAsync(0);

        await addOptionLinkFirst.ClickAsync();
        await Assertions.Expect(optionRowsFirst).ToHaveCountAsync(1);

        // The second instance's row count must be completely unaffected by the first
        // instance's add - this is the exact "module-scoped state collision" class of bug
        // (a shared closure's row array would leak the add across both instances).
        await Assertions.Expect(optionRowsSecond).ToHaveCountAsync(0);

        // Fill in distinguishable text in the first instance's new row and confirm it does
        // NOT appear anywhere in the second instance - proves the two Vue app instances
        // (and their reactive `state.options` arrays) are genuinely separate objects, not
        // just visually-separate DOM nodes bound to one shared array.
        var textInputFirst = optionRowsFirst.Nth(0).Locator("input[type='text']").First;
        await textInputFirst.FillAsync("first-instance-only-option");

        await addOptionLinkSecond.ClickAsync();
        await Assertions.Expect(optionRowsSecond).ToHaveCountAsync(1);

        var textInputSecond = optionRowsSecond.Nth(0).Locator("input[type='text']").First;
        await Assertions.Expect(textInputSecond).Not.ToHaveValueAsync("first-instance-only-option");
        await textInputSecond.FillAsync("second-instance-only-option");

        // Round-trip: publish the page and confirm both widgets' independently-typed
        // values persisted to their own hidden JSON input, not merged/duplicated/lost.
        // Publishing redirects to the content item's list page (Contents/ContentItems);
        // re-open the same item for edit from there rather than assuming a specific
        // post-publish URL shape.
        await page.ClickPublishAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/FormsFieldEditorTestPage");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Forms Field Editor AJAX Test" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedWidgets = WidgetOfType(page, "FormsFieldEditorTestWidget");
        await Assertions.Expect(reloadedWidgets).ToHaveCountAsync(2);

        var reloadedFirst = reloadedWidgets.Nth(0);
        var reloadedSecond = reloadedWidgets.Nth(1);
        var reloadedFirstRow = reloadedFirst.Locator(".select-options-table tbody tr").First;
        var reloadedSecondRow = reloadedSecond.Locator(".select-options-table tbody tr").First;

        await Assertions.Expect(reloadedFirstRow.Locator("input[type='text']").First).ToHaveValueAsync("first-instance-only-option");
        await Assertions.Expect(reloadedSecondRow.Locator("input[type='text']").First).ToHaveValueAsync("second-instance-only-option");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
