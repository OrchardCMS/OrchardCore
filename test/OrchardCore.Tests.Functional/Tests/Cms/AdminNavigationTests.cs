using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminNavigationTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    private const string Password = "Orchard1!";

    public AdminNavigationTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AdminNavigation_Default_NotUseAdminQueryParameter()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        // Admin links persisted by TheAdmin include data-admin-hash and local admin hrefs.
        var adminLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/\"]").First;
        await adminLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("?admin=", page.Url, StringComparison.OrdinalIgnoreCase);

        var selectedNavHash = await page.EvaluateAsync<string>("""
            () => {
                const tenant = document.documentElement.getAttribute('data-tenant') || 'default';
                return sessionStorage.getItem(`${tenant}-selectedNavHash`);
            }
            """);

        Assert.False(string.IsNullOrWhiteSpace(selectedNavHash));

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminRoot_Default_NotKeepPreviousMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Features");

        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/Admin/Features\"]").First;
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);

        await page.GotoAndAssertOkAsync("/Admin");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_ContentTypeEdit_KeepsContentTypeMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();

        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var contentItemsLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems\"]");
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeContentItemsItem = contentItemsLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await articleLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Edit", page.Url, StringComparison.OrdinalIgnoreCase);
        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_DirectNavigation_NotKeepPreviousMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();

        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/Admin/Features\"]").First;
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        // Clicking the nav link stores its hash in the admin preferences cookie.
        await articleLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);

        // Direct navigation (bookmark, typed URL, in-page link) to a page whose URL
        // exactly matches another menu item must be selected instead of the previously
        // clicked menu item, if a new browser session is used.
        await page.SessionStorage.ClearAsync();
        await page.GotoAndAssertOkAsync("/Admin/Features");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_ContentItemEditBookmark_ActivatesContentTypeMenuItem()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Find the edit URL of the "About" article from its content type list page.
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Article");
        var aboutEditUrl = await page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).GetAttributeAsync("href");
        Assert.Contains("/Edit", aboutEditUrl, StringComparison.OrdinalIgnoreCase);

        // Simulate a bookmark: no admin preferences cookie, so no clicked-hash fallback exists.
        var prefsCookie = (await page.Context.CookiesAsync())
            .FirstOrDefault(c => c.Name.EndsWith("-adminPreferences", StringComparison.Ordinal));
        if (prefsCookie != null)
        {
            await page.Context.ClearCookiesAsync(new() { Name = prefsCookie.Name });
        }

        await page.GotoAndAssertOkAsync(aboutEditUrl);

        // The edit page declares the Article list page as its owner, so the Article menu item
        // must be active even without any URL match or stored preference.
        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var contentItemsLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems\"]");
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeContentItemsItem = contentItemsLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_LinkToContentItemEdit_KeepsLinkMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        // "Main Menu" is an admin menu link pointing directly at the edit page of a Menu content
        // item, so it must stay active instead of the list page of the Menu content type, which
        // that edit page declares as its owner.
        var mainMenuLink = page.GetByRole(AriaRole.Link, new() { Name = "Main Menu", Exact = true });
        await mainMenuLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Edit", page.Url, StringComparison.OrdinalIgnoreCase);

        var activeMainMenuItem = mainMenuLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var menusLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Menu\"]");
        var activeMenusItem = menusLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeMainMenuItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeMenusItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task MenuPermissions_EditorCanManageMenuItemsWithoutPublishing()
    {
        const string menuEditorRole = "MenuEditor";
        const string noMenuAccessRole = "NoMenuAccess";
        const string menuEditorUser = "menu-editor";
        const string noMenuAccessUser = "no-menu-access";

        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/Edit/Menu");
        await page.Locator("#ContentTypeDefinition_Securable").CheckAsync();
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await CreateRoleWithPermissionsAsync(
            page,
            menuEditorRole,
            "AccessAdminPanel",
            "ListContent_Menu",
            "Edit_Menu");
        await CreateRoleWithPermissionsAsync(page, noMenuAccessRole, "AccessAdminPanel");

        await UserHelper.CreateUserAsync(page, string.Empty, menuEditorUser, "menu-editor@test.com", Password, menuEditorRole);
        await UserHelper.CreateUserAsync(page, string.Empty, noMenuAccessUser, "no-menu-access@test.com", Password, noMenuAccessRole);

        await UserHelper.LoginAsAsync(page, string.Empty, menuEditorUser, Password);
        await page.GotoAndAssertOkAsync("/Admin");

        var menusLink = page.Locator("#adminMenu a[href^=\"/Admin/Contents/ContentItems/Menu\"]");
        await Assertions.Expect(menusLink).ToHaveCountAsync(1);

        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();
        await Assertions.Expect(menusLink).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Menu");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Manage Content" })).ToBeVisibleAsync();

        var editUrl = await page.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true })
            .First
            .GetAttributeAsync("href");

        Assert.NotNull(editUrl);
        await page.GotoAndAssertOkAsync(editUrl);

        var addMenuItemUrl = await page.Locator("a[href*='/Admin/Menu/Create/LinkMenuItem']")
            .First
            .GetAttributeAsync("href");

        Assert.NotNull(addMenuItemUrl);
        await page.GotoAndAssertOkAsync(addMenuItemUrl);
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "New Link Menu Item" })).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Menu");
        await Assertions.Expect(page.Locator("form[action*='/Publish']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("form[action*='/Delete']")).ToHaveCountAsync(0);

        var contentItemId = new Uri(new Uri(Fixture.BaseUrl), editUrl)
            .Segments
            .SkipWhile(segment => !string.Equals(segment, "ContentItems/", StringComparison.Ordinal))
            .Skip(1)
            .First()
            .TrimEnd('/');

        await SubmitContentActionAsync(page, $"/Admin/Contents/ContentItems/{contentItemId}/Publish");
        Assert.Contains("/Error/403", page.Url, StringComparison.OrdinalIgnoreCase);

        await UserHelper.LoginAsAsync(page, string.Empty, noMenuAccessUser, Password);
        await page.GotoAndAssertOkAsync("/Admin");
        await Assertions.Expect(page.Locator("#adminMenu a[href^=\"/Admin/Contents/ContentItems/Menu\"]")).ToHaveCountAsync(0);

        await page.GotoAsync("/Admin/Contents/ContentItems/Menu");
        Assert.Contains("/Error/403", page.Url, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    private static async Task CreateRoleWithPermissionsAsync(
        IPage page,
        string roleName,
        params string[] permissionNames)
    {
        await page.GotoAndAssertOkAsync("/Admin/Roles/Create");
        await page.Locator("input[name='RoleName']").FillAsync(roleName);
        await page.ClickCreateAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync($"/Admin/Roles/Edit/{Uri.EscapeDataString(roleName)}");

        foreach (var permissionName in permissionNames)
        {
            var permission = page.Locator($"input[id='Checkbox.{permissionName}']");
            await permission.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await permission.CheckAsync();
        }

        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task SubmitContentActionAsync(IPage page, string action)
    {
        await Task.WhenAll(
            page.WaitForURLAsync("**/Error/403**"),
            page.EvaluateAsync(
                """
                action => {
                    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                    const form = document.createElement("form");
                    form.method = "post";
                    form.action = action;

                    const tokenInput = document.createElement("input");
                    tokenInput.type = "hidden";
                    tokenInput.name = "__RequestVerificationToken";
                    tokenInput.value = token;

                    form.appendChild(tokenInput);
                    document.body.appendChild(form);
                    form.submit();
                }
                """,
                action));

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
