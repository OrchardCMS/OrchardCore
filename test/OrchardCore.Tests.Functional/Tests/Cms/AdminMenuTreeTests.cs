using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based nested drag-and-drop tree editor on the Admin
// Menu node list (src/OrchardCore.Modules/OrchardCore.AdminMenu/Views/Node/
// List.cshtml) - genuinely nested <ol>/<li> markup (unlike Menu/Taxonomies'
// flat depth-tagged list), persisted via one fetch() POST per move plus a
// full page reload. The Blog recipe's "Admin menus" tree ships three root
// nodes ("Blog", "Main Menu", "Content") where "Content" is a placeholder
// with its own nested children ("Content Items" among them) - node text
// overlaps ("Content" vs "Content Items"), so nodes are targeted by their
// recipe-authored (and therefore stable across runs) UniqueId rather than
// by visible text.
public sealed class AdminMenuTreeTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    private const string BlogNodeId = "7b293d57056a4eebb3713f07f12c65d8";
    private const string MainMenuNodeId = "5118cecfde834dacb26ac08980f1b5a7";
    private const string ContentNodeId = "3e590d44f8704e4588e272dd966ce291";
    private const string ContentItemsNodeId = "7b293d57056a4eebb3713f07f12c65d9";

    public AdminMenuTreeTests(BlogFixture fixture) : base(fixture) { }

    private static ILocator TreeNode(IPage page, string treeNodeId)
        => page.Locator($"li.menu-item[data-treenode-id='{treeNodeId}']").First;

    private static async Task OpenAdminMenusTreeAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/AdminMenu/List");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Admin menus" })
            .Locator("a").Filter(new LocatorFilterOptions { HasText = "Edit Nodes" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("#menu").WaitForAsync();
    }

    // A move POSTs to MoveNode and then calls location.reload() from the fetch
    // continuation (see admin-menu-node-list.ts), so a drag is only settled once
    // that reloaded document has replaced the current one.
    //
    // Waiting on "#menu" cannot see that: SortableJS has already moved the node
    // in the DOM optimistically, so #menu is present, already showing the node in
    // its new place, while the POST is still in flight. Asserting there passes
    // against the pre-reload DOM without the server having stored anything, and
    // if the navigation instead lands mid-assertion the tree is momentarily being
    // replaced and the same assertion sees no nodes at all - which is the flake.
    //
    // So tag the current document and wait for the tag to be gone. Only the
    // reloaded document lacks it, which both pins the assertions to what the
    // server actually persisted and makes a drag that never registered fail here,
    // naming the drag, rather than further down as a puzzling count of 0.
    private static async Task DragAndWaitForReloadAsync(IPage page, ILocator source, ILocator target)
    {
        await page.EvaluateAsync("() => document.documentElement.setAttribute('data-oc-pre-reload', '')");

        await page.DragAsync(source, target);

        await page.Locator("html:not([data-oc-pre-reload])").WaitForAsync();
        await page.Locator("#menu").WaitForAsync();
    }

    [Fact]
    public async Task AdminMenuTree_DragRootNodeIntoPlaceholder_ReparentsAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenAdminMenusTreeAsync(page);

        // Dropping directly onto an existing child ("Content Items") rather
        // than onto "Content"'s own (empty-looking, ambiguously-sized) child
        // list container reliably lands the drop inside that same nested
        // list, right next to it.
        await DragAndWaitForReloadAsync(
            page,
            TreeNode(page, BlogNodeId).Locator(".menu-item-title"),
            TreeNode(page, ContentItemsNodeId).Locator(".menu-item-title"));

        // "Blog" now lives inside "Content"'s own nested list rather than at the
        // root. The counts are asserted through Expect so that they are retried:
        // the reloaded tree is rendered by the server, not by the drag, so it can
        // still be arriving when the first assertion runs.
        await Assertions.Expect(TreeNode(page, ContentNodeId).Locator("> ol.menu-item-links li.menu-item[data-treenode-id='" + BlogNodeId + "']"))
            .ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#menu > li.menu-item[data-treenode-id='" + BlogNodeId + "']"))
            .ToHaveCountAsync(0);

        // Move it back out to the root, right where it started, so re-running
        // this test isn't affected by a leftover reparent from a prior run.
        // Dropping onto the root <ol> itself is unreliable, since its
        // bounding box also visually contains nested child lists - targeting
        // a root-level sibling ("Main Menu") instead reliably lands the drop
        // in the root list.
        await DragAndWaitForReloadAsync(
            page,
            TreeNode(page, BlogNodeId).Locator(".menu-item-title"),
            TreeNode(page, MainMenuNodeId).Locator(".menu-item-title"));

        await Assertions.Expect(page.Locator("#menu > li.menu-item[data-treenode-id='" + BlogNodeId + "']"))
            .ToHaveCountAsync(1);

        await page.CloseAsync();
    }

    // Covers the vanilla-JS trigger/remove-icon handlers on the Link/Placeholder
    // node editors (src/OrchardCore.Modules/OrchardCore.AdminMenu/Views/Items/
    // LinkAdminNode.Fields.TreeEdit.cshtml) - the icon picker widget itself
    // (fontawesome-iconpicker) stays jQuery-based and isn't exercised here; this
    // only covers the surrounding code that was actually converted.
    [Fact]
    public async Task AdminMenuIconPicker_RemoveButton_ClearsIconAndTriggerOpensModal()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenAdminMenusTreeAsync(page);

        await TreeNode(page, BlogNodeId).Locator("a.btn-primary").Filter(new LocatorFilterOptions { HasText = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var sampleIcon = page.Locator("i[id^='sample-icon-']");
        await sampleIcon.WaitForAsync();

        // The Blog node ships with IconClass "fas fa-rss" (see blog.recipe.json).
        Assert.Contains("fa-rss", await sampleIcon.GetAttributeAsync("class"));

        // The hidden IconClass input's id is only known once rendered (it may
        // carry a model-binding prefix) - the remove/trigger buttons carry it
        // as data-related-node, which is the most reliable way to find it.
        var iconFieldId = await page.Locator("button.remove-icon").GetAttributeAsync("data-related-node");
        await page.Locator("button.remove-icon").ClickAsync();

        Assert.Equal(" ", await page.Locator("i[id^='sample-icon-']").GetAttributeAsync("class"));
        Assert.Equal(string.Empty, await page.Locator($"#{iconFieldId}").InputValueAsync());

        await page.Locator("button.icon-picker-trigger").ClickAsync();
        await Assertions.Expect(page.Locator("#iconPickerModal")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        await page.CloseAsync();
    }

    // Covers the shared Vue 3 permission-picker.ts component (initPermissionPicker,
    // src/.scripts/bloom/components/permission-picker.ts) mounted on LinkAdminNode's
    // #PermissionPicker element - vue-multiselect-backed search/select, an "arrayOfItems"
    // list with per-item remove buttons, and a hidden SelectedPermissionNames input kept in
    // sync via a computed property. Same component (byte-identical Vue instance) is reused
    // by OrchardCore.Menu's MenuItemPermissionPart.Edit.cshtml.
    [Fact]
    public async Task AdminMenuPermissionPicker_SelectAndRemove_UpdatesHiddenInput()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenAdminMenusTreeAsync(page);

        await TreeNode(page, BlogNodeId).Locator("a.btn-primary").Filter(new LocatorFilterOptions { HasText = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var picker = page.Locator("#PermissionPicker");
        await picker.WaitForAsync();

        // The Blog node ships with an empty PermissionNames array (see blog.recipe.json),
        // so the picker starts with no selected-permission list items.
        var itemList = picker.Locator("ul.permission-picker-default__list");
        await Assertions.Expect(itemList).ToBeHiddenAsync();

        var hiddenInputId = await picker.GetAttributeAsync("data-selected-names-input-id");
        Assert.False(string.IsNullOrEmpty(hiddenInputId));
        var hiddenInput = page.Locator($"#{hiddenInputId}");
        Assert.Equal(string.Empty, await hiddenInput.InputValueAsync());

        // Open the vue-multiselect dropdown and select its first available option.
        await picker.Locator(".multiselect").ClickAsync();
        var firstOption = picker.Locator(".multiselect__option").First;
        await firstOption.WaitForAsync();
        var selectedDisplayText = (await firstOption.TextContentAsync())!.Trim();
        await firstOption.ClickAsync();

        await Assertions.Expect(itemList).ToBeVisibleAsync();
        var listItems = itemList.Locator("li.list-group-item");
        await Assertions.Expect(listItems).ToHaveCountAsync(1);
        Assert.Contains(selectedDisplayText, (await listItems.First.TextContentAsync())!);

        var selectedName = await hiddenInput.InputValueAsync();
        Assert.False(string.IsNullOrEmpty(selectedName));

        // Removing it via the list item's own delete button clears the hidden input again.
        await listItems.First.Locator("button.permission-picker-default__list-item__delete").ClickAsync();
        await Assertions.Expect(itemList).ToBeHiddenAsync();
        Assert.Equal(string.Empty, await hiddenInput.InputValueAsync());

        await page.CloseAsync();
    }
}
