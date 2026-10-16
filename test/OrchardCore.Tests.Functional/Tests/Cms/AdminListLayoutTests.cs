using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Exercises view-defined lists, provider overrides, layout preferences and persisted contained-item ordering.
public sealed class AdminListLayoutTests : CmsTestBase<AdminListLayoutTestsFixture>, IClassFixture<AdminListLayoutTestsFixture>
{
    // The lists of the features the Blog recipe enables.
    private static readonly string[] _lists =
    [
        "/Admin/Users/Index",
        "/Admin/Roles/Index",
        "/Admin/ContentTypes/List",
        "/Admin/ContentTypes/ListParts",
        "/Admin/Contents/ContentItems",
        "/Admin/AdminMenu/List",
        "/Admin/DeploymentPlan/Index",
        "/Admin/Deployment/RemoteInstance/Index",
        "/Admin/Deployment/RemoteClient/Index",
        "/Admin/indexing",
        "/Admin/Layers",
        "/Admin/MediaProfiles",
        "/Admin/Placements",
        "/Admin/Queries/Index",
        "/Admin/Recipes",
        "/Admin/Shortcodes",
        "/Admin/Templates",
        "/Admin/Features",
    ];

    public AdminListLayoutTests(AdminListLayoutTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task LayoutSelector_UserPicksGrid_ListRendersViewDefinedColumnsAndKeepsTheChoice()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.SetAdminListSettingsAsync("List", allowUserSelection: true);

        // Asked for explicitly, so the list renders its rows whole whatever layout the other test gave the site.
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?q=admin&page=1&pageSize=1&layout=List");
        await Assertions.Expect(page.Locator("ul.admin-list-list li.list-group-item:not(.text-bg-theme)", new() { HasText = "admin" }).First).ToBeVisibleAsync();

        var selector = page.Locator(".admin-list-layout-selector");
        await Assertions.Expect(selector).ToHaveCountAsync(1);

        await selector.Locator("a[title='Grid']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("layout=Grid", page.Url);
        var query = QueryHelpers.ParseQuery(new Uri(page.Url).Query);
        Assert.Equal("admin", query["q"]);
        Assert.Equal("1", query["page"]);
        Assert.Equal("1", query["pageSize"]);
        await Assertions.Expect(page.Locator("input[name='Options.SearchText']")).ToHaveValueAsync("admin");

        // The view declares the headers; cells reuse the shapes returned by the row display drivers.
        Assert.Equal(["", "User", "Roles", "Actions"], await HeadersAsync(page));
        await Assertions.Expect(page.Locator(".admin-list-grid .admin-list-grid-row .admin-list-column-user", new() { HasText = "admin" }).First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("label#items")).ToHaveTextAsync(new Regex("^1 item"));

        // The choice is kept for the list, so it opens the same way without the query string.
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);

        await page.Locator(".admin-list-layout-selector a[title='List']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-list")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(0);

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems?layout=Grid");
        var title = page.Locator(".admin-list-grid-row .admin-list-column-title a").First;
        await Assertions.Expect(title).ToBeVisibleAsync();
        var titleText = (await title.InnerTextAsync()).Trim();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems?layout=List");
        await Assertions.Expect(page.Locator("ul.admin-list-list li.list-group-item", new() { HasText = titleText }).First).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "List BlogPost Items", Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.Locator("#ci-sortable .admin-list")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#ci-sortable input[type='checkbox']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#ci-sortable .admin-list-grid-row, #ci-sortable li.list-group-item").First).ToBeVisibleAsync();

        // The features render one list per category and offer the layout once, beside their filters.
        await page.GotoAndAssertOkAsync("/Admin/Features?layout=Grid");
        await Assertions.Expect(page.Locator(".admin-list-layout-selector")).ToHaveCountAsync(1);
        Assert.True(await page.Locator(".admin-list").CountAsync() > 1, "Expected the features to render one list per category.");
        Assert.Equal(["", "Name", "Dependencies", "Status", "Actions"], await HeadersAsync(page));
        await Assertions.Expect(page.Locator(".admin-list-grid-row .admin-list-column-name").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("admin-list, admin-list-grid, admin-list-list, admin-list-column")).ToHaveCountAsync(0);
        await VerifyFeatureSearchAsync(page);

        await page.GotoAndAssertOkAsync("/Admin/Features?layout=List");
        await Assertions.Expect(page.Locator("ul.admin-list-list li.list-group-item").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(0);
        await VerifyFeatureSearchAsync(page);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    private static async Task VerifyFeatureSearchAsync(IPage page)
    {
        var rows = page.Locator(".admin-list [data-filter-value]");
        Assert.True(await rows.CountAsync() > 0);
        Assert.Equal(await rows.CountAsync(), await page.Locator(".admin-list [data-filter-value][data-is-enabled][data-is-on-demand][data-is-always-enabled]").CountAsync());
        var search = page.Locator("#search-box");
        await search.FillAsync("no-matching-feature-for-admin-list-test");
        await Assertions.Expect(page.Locator(".admin-list [data-filter-value]:visible")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#list-alert")).ToBeVisibleAsync();
        await search.FillAsync(string.Empty);
        await Assertions.Expect(page.Locator(".admin-list [data-filter-value]:visible").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SiteLayout_Grid_RendersEveryListWithViewDefinedColumns()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.SetAdminListSettingsAsync("Grid", allowUserSelection: false);

        foreach (var url in _lists)
        {
            await page.GotoAndAssertOkAsync(url);

            // A list only renders as a grid when its view or providers declare columns. The headers are read rather
            // than seen: a list in a narrow column, e.g. the layers beside their zones, stacks its rows and hides them.
            var headers = await HeadersAsync(page);

            Assert.True(headers.Contains("Actions"), $"Expected {url} to render its list as a grid, but its headers are [{string.Join(", ", headers)}].");
        }

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColumnsProvider_ViewDefaults_AddsRemovesReordersAndRestylesColumns()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();
        await page.SetAdminListSettingsAsync("Grid", allowUserSelection: false);

        await page.GotoAndAssertOkAsync("/Admin/Users/Index?columns=override");

        Assert.Equal(["Assigned roles", "Account", "Actions"], await HeadersAsync(page));
        await Assertions.Expect(page.Locator(".admin-list-grid .admin-list-column-select")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator(".admin-list-grid-row .provider-roles")).ToContainTextAsync("Administrator");
        await Assertions.Expect(page.Locator(".admin-list-grid-row .admin-list-column-account")).ToContainTextAsync("admin");
        Assert.Contains("12rem", await page.Locator(".admin-list-grid-table").GetAttributeAsync("style"));
        await Assertions.Expect(page.Locator(".admin-list-grid-row .provider-roles")).ToHaveClassAsync(new Regex("admin-list-align-center"));

        await page.GotoAndAssertOkAsync("/Admin/Users/Index?columns=override&q=no-matching-admin-list-user");
        Assert.Equal(["Assigned roles", "Account", "Actions"], await HeadersAsync(page));
        await Assertions.Expect(page.Locator(".admin-list-grid-row[role='row']")).ToHaveCountAsync(0);

        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        Assert.Equal(["", "User", "Roles", "Actions"], await HeadersAsync(page));
        await page.GotoAndAssertOkAsync("/Admin/Roles/Index?columns=override");
        Assert.Equal(["Name", "Kind", "Actions"], await HeadersAsync(page));

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task OptionalLists_ListAndGrid_RenderTheirDeclaredColumns()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();
        string[] features =
        [
            "OrchardCore.BackgroundTasks",
            "OrchardCore.AuditTrail",
            "OrchardCore.Notifications",
            "OrchardCore.OpenId.Management",
            "OrchardCore.RateLimits",
            "OrchardCore.Tenants",
            "OrchardCore.Sitemaps",
            "OrchardCore.Workflows",
        ];
        foreach (var feature in features)
        {
            await page.EnableFeatureAsync(string.Empty, feature);
        }

        await page.GotoAndAssertOkAsync("/Admin/Workflows/Types/EditProperties");
        await page.GetByLabel("Name", new() { Exact = true }).FillAsync("Functional list workflow");
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GotoAndAssertOkAsync("/Admin/Workflows/Types");
        var editUrl = await page.GetByRole(AriaRole.Link, new() { Name = "Functional list workflow", Exact = true }).GetAttributeAsync("href");
        var workflowTypeId = new Uri(new Uri(page.Url), editUrl).Segments.Last();
        string[] urls =
        [
            "/Admin/BackgroundTasks",
            "/Admin/AuditTrail",
            "/Admin/Notifications",
            "/Admin/OpenId/Application",
            "/Admin/OpenId/Scope",
            "/Admin/RateLimits",
            "/Admin/Tenants",
            "/Admin/SitemapsCache/List",
            "/Admin/Workflows/Types",
            $"/Admin/Workflows/Types/{workflowTypeId}/Instances/Index",
        ];
        string[] layouts = ["List", "Grid"];
        foreach (var layout in layouts)
        {
            await page.SetAdminListSettingsAsync(layout, allowUserSelection: false);
            foreach (var url in urls)
            {
                await page.GotoAndAssertOkAsync(url);
                await Assertions.Expect(page.Locator($".admin-list-{layout.ToLowerInvariant()}")).ToHaveCountAsync(1);
                if (layout == "Grid")
                {
                    Assert.Contains("Actions", await HeadersAsync(page));
                }
            }
        }

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ListPart_OrderingEnabled_ForcesListAndPersistsDraggedRows()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();
        await page.SetAdminListSettingsAsync("Grid", allowUserSelection: true);

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/Blog/ContentParts/ListPart/Edit");
        await page.GetByLabel("Enable Ordering", new() { Exact = true }).CheckAsync();
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems");
        await page.GetByRole(AriaRole.Link, new() { Name = "List BlogPost Items", Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var listUrl = QueryHelpers.AddQueryString(page.Url, "layout", "Grid");

        await page.Locator("a[href*='ListPart.ContainerId'][href*='BlogPost']").ClickAsync();
        await page.Locator("#TitlePart_Title").FillAsync("Second ordered blog post");
        await page.ClickPublishAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GotoAndAssertOkAsync(listUrl);

        var rows = page.Locator("#ci-sortable .admin-list-list > li");
        await Assertions.Expect(rows).ToHaveCountAsync(2);
        await Assertions.Expect(page.Locator("#ci-sortable .admin-list-grid")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#ci-sortable .admin-list-layout-selector")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#ci-sortable input[type='checkbox']")).ToHaveCountAsync(0);
        await Assertions.Expect(rows.Locator(".ui-sortable-handle")).ToHaveCountAsync(2);
        var firstTitle = await rows.Nth(0).Locator(".title a").First.InnerTextAsync();
        var secondTitle = await rows.Nth(1).Locator(".title a").First.InnerTextAsync();
        var handle = rows.Nth(0).Locator(".ui-sortable-handle");
        await handle.ScrollIntoViewIfNeededAsync();
        var firstBox = await handle.BoundingBoxAsync();
        var secondBox = await rows.Nth(1).BoundingBoxAsync();
        Assert.NotNull(firstBox);
        Assert.NotNull(secondBox);

        await page.Mouse.MoveAsync(firstBox.X + (firstBox.Width / 2), firstBox.Y + (firstBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(secondBox.X + (secondBox.Width / 2), secondBox.Y + (secondBox.Height / 2) + 5, new() { Steps = 10 });
        var response = await page.RunAndWaitForResponseAsync(
            () => page.Mouse.UpAsync(),
            value => value.Request.Method == "POST" && value.Url.Contains("/Lists/Order", StringComparison.OrdinalIgnoreCase));

        Assert.True(response.Ok, $"Ordering failed with HTTP {response.Status}.");
        var form = QueryHelpers.ParseQuery(response.Request.PostData);
        Assert.Equal("0", form["oldIndex"]);
        Assert.Equal("1", form["newIndex"]);
        await page.GotoAndAssertOkAsync(listUrl);
        await Assertions.Expect(rows.Nth(0).Locator(".title a").First).ToHaveTextAsync(secondTitle);
        await Assertions.Expect(rows.Nth(1).Locator(".title a").First).ToHaveTextAsync(firstTitle);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    // The headers of the first grid of the page.
    private static async Task<string[]> HeadersAsync(IPage page)
    {
        var headers = await page.Locator(".admin-list-grid").First.Locator(".admin-list-grid-header [role='columnheader']").AllTextContentsAsync();

        return headers.Select(header => header.Trim()).ToArray();
    }
}

public sealed class AdminListLayoutTestsFixture : CmsRecipeFixture
{
    public AdminListLayoutTestsFixture()
        : base(builder => builder.Services.AddOrchardCore()
            .ConfigureServices(services => services.AddAdminListColumnsProvider<ColumnOverridesProvider>()))
    {
    }

    protected override string RecipeName => "Blog";

    private sealed class ColumnOverridesProvider : IAdminListColumnsProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ColumnOverridesProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task PopulateAsync(AdminListColumnsContext context)
        {
            if (context.ListName != "Users" || _httpContextAccessor.HttpContext.Request.Query["columns"] != "override")
            {
                return Task.CompletedTask;
            }

            context.Remove("Select");
            context.Remove("User");
            var roles = context.Find("Roles");
            roles.Title = new("Assigned roles", "Assigned roles");
            roles.Position = "10";
            roles.Width = "12rem";
            roles.CssClass = "provider-roles";
            roles.Alignment = AdminListColumnAlignment.Center;
            context.Columns.Add(new()
            {
                Name = "Account",
                Title = new("Account", "Account"),
                Position = "25",
                Zones = ["User"],
            });

            return Task.CompletedTask;
        }
    }
}
