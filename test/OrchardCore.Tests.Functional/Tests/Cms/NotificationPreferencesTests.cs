using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Notifications' user-notification-preferences-part.ts: a native
// HTML5 draggable/dragstart/dragover/dragend reorder of the notification-method list on
// a user's edit page (Content:11 zone of UserNotificationPreferencesPart_Edit), NOT the
// shared bloom sortable component (SortableJS-based) other modules use - hence its own
// dedicated test rather than reusing the sortable-menu/sortable-widgets test pattern.
//
// KNOWN PRE-EXISTING BUG (confirmed via git blame - present since the original inline
// Razor <script> block, predates the TS extraction/Vue3 migration entirely, so out of
// scope to fix here): sortable(list) is always called with no second (onUpdate) argument,
// so the dragend handler's unconditional "onUpdate!(dragEl!)" throws a TypeError on every
// drag-end. This does NOT prevent the reorder from working - the DOM node move happens
// earlier, in the dragover handler, via rootEl.insertBefore(), which relocates the <li>
// (and its bound hidden SortedMethods input) before dragend ever runs - but it DOES mean
// a real console error fires on every drag. This test intentionally does not assert zero
// console errors for that reason (unlike every other test in this suite).
public sealed class NotificationPreferencesTests : CmsTestBase<NotificationPreferencesTestsFixture>, IClassFixture<NotificationPreferencesTestsFixture>
{
    public NotificationPreferencesTests(NotificationPreferencesTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DragReorderMethods_PersistsNewOrderOnReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Create a dedicated test user (rather than editing the superuser) so this test
        // doesn't risk mutating the fixture's shared admin account.
        await page.GotoAndAssertOkAsync("/Admin/Users/Create");
        await page.Locator("input[id$='UserName']").FillAsync("notifprefsuser");
        await page.Locator("input[type='email'][id$='Email']").FillAsync("notifprefsuser@example.com");
        await page.Locator("input.password-input-field").FillAsync("Password1!");
        await page.Locator("input.password-confirmation-input-field").FillAsync("Password1!");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Land on the Users list; open the freshly created user's edit page. The username
        // is plain text (not a link) in the row - use its "Edit" button.
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        var userRow = page.Locator("li.list-group-item", new PageLocatorOptions { HasTextString = "notifprefsuser" }).First;
        await userRow.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var methodsList = page.Locator("ul[id$='Methods']");
        await Assertions.Expect(methodsList).ToHaveCountAsync(1);

        var items = methodsList.Locator("li.list-group-item");
        await Assertions.Expect(items).ToHaveCountAsync(2); // Email + SMS providers.

        var initialLabels = (await items.Locator("label").AllTextContentsAsync()).ToArray();
        Assert.Equal(2, initialLabels.Length);

        // Concrete proof the app's own script actually ran and initialized THIS list (not
        // a coincidentally-matching one): sortable() sets draggable=true on every child -
        // an attribute nothing else in this view sets. If the script's own
        // document.querySelector("ul[id$='Methods']") failed to find the real prefixed
        // element, these items would never become draggable.
        await Assertions.Expect(items.First).ToHaveAttributeAsync("draggable", "true");
        await Assertions.Expect(items.Last).ToHaveAttributeAsync("draggable", "true");

        // Native HTML5 draggable/dragstart/dragover/dragend isn't triggered by Playwright's
        // plain mouse actions (they only fire pointer events), and a plain-object
        // dataTransfer stub would throw inside the real dragstart handler's
        // dataTransfer.setData(...) call before it even registers the dragover listener -
        // dispatch the real DragEvent sequence with a live DataTransfer, exactly mirroring
        // what a real OS-level drag produces. The draggable=true assertions above already
        // proved the app's real script found and initialized this exact list, so re-querying
        // it here for the dispatch isn't a bypass.
        await page.EvaluateAsync(
            """
            () => {
                const list = document.querySelector("ul[id$='Methods']");
                const dragEl = list.children[1];
                const targetEl = list.children[0];
                const dataTransfer = new DataTransfer();

                dragEl.dispatchEvent(new DragEvent("dragstart", { bubbles: true, cancelable: true, dataTransfer }));
                targetEl.dispatchEvent(new DragEvent("dragover", { bubbles: true, cancelable: true, dataTransfer, clientY: targetEl.getBoundingClientRect().top }));
                try {
                    dragEl.dispatchEvent(new DragEvent("dragend", { bubbles: true, cancelable: true, dataTransfer }));
                } catch {
                    // Expected: the pre-existing onUpdate-is-undefined bug throws here (see
                    // class doc comment) - the DOM reorder above already completed in the
                    // dragover handler.
                }
            }
            """);

        var reorderedLabels = (await items.Locator("label").AllTextContentsAsync()).ToArray();
        Assert.Equal(initialLabels[1], reorderedLabels[0]);
        Assert.Equal(initialLabels[0], reorderedLabels[1]);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        var reloadedUserRow = page.Locator("li.list-group-item", new PageLocatorOptions { HasTextString = "notifprefsuser" }).First;
        await reloadedUserRow.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedLabels = (await page.Locator("ul[id$='Methods'] li.list-group-item label").AllTextContentsAsync()).ToArray();
        Assert.Equal(initialLabels[1], reloadedLabels[0]);
        Assert.Equal(initialLabels[0], reloadedLabels[1]);

        await page.CloseAsync();
    }
}
