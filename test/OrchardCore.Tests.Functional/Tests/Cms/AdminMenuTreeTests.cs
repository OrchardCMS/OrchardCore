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
        await page.DragAsync(TreeNode(page, BlogNodeId).Locator(".menu-item-title"), TreeNode(page, ContentItemsNodeId).Locator(".menu-item-title"));

        // A successful move triggers location.reload() - wait for the tree to
        // re-render, then confirm "Blog" now lives inside "Content"'s own
        // nested list rather than at the root.
        await page.Locator("#menu").WaitForAsync();
        Assert.Equal(1, await TreeNode(page, ContentNodeId).Locator("> ol.menu-item-links li.menu-item[data-treenode-id='" + BlogNodeId + "']").CountAsync());
        Assert.Equal(0, await page.Locator("#menu > li.menu-item[data-treenode-id='" + BlogNodeId + "']").CountAsync());

        // Move it back out to the root, right where it started, so re-running
        // this test isn't affected by a leftover reparent from a prior run.
        // Dropping onto the root <ol> itself is unreliable, since its
        // bounding box also visually contains nested child lists - targeting
        // a root-level sibling ("Main Menu") instead reliably lands the drop
        // in the root list.
        await page.DragAsync(TreeNode(page, BlogNodeId).Locator(".menu-item-title"), TreeNode(page, MainMenuNodeId).Locator(".menu-item-title"));
        await page.Locator("#menu").WaitForAsync();

        Assert.Equal(1, await page.Locator("#menu > li.menu-item[data-treenode-id='" + BlogNodeId + "']").CountAsync());

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
}
