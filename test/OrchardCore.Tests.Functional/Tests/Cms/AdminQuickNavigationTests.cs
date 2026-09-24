using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminQuickNavigationTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public AdminQuickNavigationTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task QuickNavigation_AlternateAdminTheme_UsesModuleAssetsAndAllowsStyleOverrides()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/Themes");
        try
        {
            await page.Locator("form[action*='SetCurrentTheme/AdminThemeSample'] button").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-admin-theme", "AdminThemeSample");
            await Assertions.Expect(page.Locator("script[src*='/TheAdmin/'], link[href*='/TheAdmin/']")).ToHaveCountAsync(0);
            await Assertions.Expect(page.Locator("script[src*='/OrchardCore.Admin/Scripts/quick-navigation/']")).ToHaveCountAsync(1);
            await Assertions.Expect(page.Locator("link[href*='/OrchardCore.Admin/Styles/quick-navigation']")).ToHaveCountAsync(1);

            await page.Locator("#adminQuickNavigationToggle").ClickAsync();
            var modal = page.Locator("#adminQuickNavigationModal");
            var input = page.Locator("#adminQuickNavigationInput");
            await Assertions.Expect(modal).ToBeVisibleAsync();
            await Assertions.Expect(input).ToBeFocusedAsync();
            await input.FillAsync("features");
            var option = modal.Locator("[role=option]").First;
            await Assertions.Expect(option.Locator(".admin-quick-navigation-title")).ToHaveTextAsync("Features");
            await Assertions.Expect(modal.Locator(".admin-quick-navigation-input")).ToHaveCSSAsync("display", "flex");

            await page.AddStyleTagAsync(new()
            {
                Content = ".admin-quick-navigation .admin-quick-navigation-results .admin-quick-navigation-title { font-weight: 400; }",
            });
            await Assertions.Expect(option.Locator(".admin-quick-navigation-title")).ToHaveCSSAsync("font-weight", "400");

            await page.Keyboard.PressAsync("Escape");
            await Assertions.Expect(modal).ToBeHiddenAsync();
            await page.Keyboard.PressAsync("Control+k");
            await input.FillAsync("features");
            await Task.WhenAll(
                page.WaitForURLAsync("**/Admin/Features**"),
                page.Keyboard.PressAsync("Enter"));
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-admin-theme", "AdminThemeSample");
        }
        finally
        {
            await page.GotoAndAssertOkAsync("/Admin/Themes");
            await page.Locator("form[action*='SetCurrentTheme/TheAdmin'] button").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickNavigation_Disabled_DoesNotRenderPaletteOrLoadModuleAssets()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        try
        {
            await page.GetByLabel("Enable quick navigation", new() { Exact = true }).UncheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.GotoAndAssertOkAsync("/Admin");
            await Assertions.Expect(page.Locator("#adminQuickNavigationToggle, #adminQuickNavigationModal")).ToHaveCountAsync(0);
            await Assertions.Expect(page.Locator("script[src*='/OrchardCore.Admin/Scripts/quick-navigation/'], link[href*='/OrchardCore.Admin/Styles/quick-navigation']")).ToHaveCountAsync(0);
        }
        finally
        {
            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await page.GetByLabel("Enable quick navigation", new() { Exact = true }).CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickNavigation_WithoutSidebar_OpensPaletteAndNavigates()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        await page.Locator("#adminMenu").EvaluateAsync("menu => menu.remove()");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickNavigationModal");
        var input = page.Locator("#adminQuickNavigationInput");

        await Assertions.Expect(modal).ToBeVisibleAsync();
        await Assertions.Expect(input).ToBeFocusedAsync();

        await input.FillAsync("features");

        var firstOption = modal.Locator("[role=option]").First;
        await Assertions.Expect(firstOption.Locator(".admin-quick-navigation-title")).ToHaveTextAsync("Features");
        await Assertions.Expect(firstOption.Locator(".admin-quick-navigation-path")).ToContainTextAsync("Tools");

        await Task.WhenAll(
            page.WaitForURLAsync("**/Admin/Features**"),
            page.Keyboard.PressAsync("Enter"));

        Assert.Contains("/Admin/Features", page.Url, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_NavbarButton_OpensAndEscapeCloses()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        var modal = page.Locator("#adminQuickNavigationModal");

        await page.Locator("#adminQuickNavigationToggle").ClickAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#adminQuickNavigationInput")).ToBeFocusedAsync();

        // With an empty term the palette lists the first menu items.
        await Assertions.Expect(modal.Locator("[role=option]").First).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(modal).ToBeHiddenAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_DiacriticTerm_MatchesTitle()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickNavigationModal");
        await Assertions.Expect(modal).ToBeVisibleAsync();

        await page.Locator("#adminQuickNavigationInput").FillAsync("fêatures");

        await Assertions.Expect(modal.Locator("[role=option]").First.Locator(".admin-quick-navigation-title")).ToHaveTextAsync("Features");

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_NoMatch_ShowsNoResults()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickNavigationModal");
        await Assertions.Expect(modal).ToBeVisibleAsync();

        await page.Locator("#adminQuickNavigationInput").FillAsync("zzzz-no-such-menu-item");

        await Assertions.Expect(modal.Locator("[role=option]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#adminQuickNavigationStatus")).ToHaveTextAsync("No destinations found");

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_IndexRequest_AlwaysReturnsUncachedDocument()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        var url = await page.Locator("#adminQuickNavigationModal").GetAttributeAsync("data-index-url");

        var first = await page.Context.APIRequest.GetAsync(url);
        Assert.Equal(200, first.Status);
        Assert.Equal("no-store", first.Headers["cache-control"]);
        Assert.False(first.Headers.ContainsKey("etag"));
        var entries = (await first.JsonAsync()).Value;
        Assert.Contains(entries.EnumerateArray(), entry =>
            entry.GetProperty("source").GetString() == "AdminMenuItemNavigationSource"
            && entry.GetProperty("title").GetString() == "Features");

        var second = await page.Context.APIRequest.GetAsync(url, new()
        {
            Headers = new Dictionary<string, string> { ["If-None-Match"] = "*" },
        });

        Assert.Equal(200, second.Status);
        Assert.Equal("no-store", second.Headers["cache-control"]);
        Assert.False(second.Headers.ContainsKey("etag"));
        Assert.Equal(entries.GetRawText(), (await second.JsonAsync()).Value.GetRawText());

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_AnonymousRequest_RequiresAuthentication()
    {
        var page = await Fixture.CreatePageAsync();

        var response = await page.Context.APIRequest.GetAsync("/Admin/QuickNavigation/Index", new() { MaxRedirects = 0 });

        Assert.Equal(302, response.Status);
        Assert.Contains("/Login", response.Headers["location"], StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_LoadFailure_ShowsErrorAndRetriesOnReopen()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        await page.RouteAsync("**/QuickNavigation/Index**", route => route.FulfillAsync(new() { Status = 503 }));
        await page.Locator("#adminQuickNavigationToggle").ClickAsync();

        var status = page.Locator("#adminQuickNavigationStatus");
        await Assertions.Expect(status).ToContainTextAsync("Unable to load navigation");
        await page.Locator("#adminQuickNavigationInput").FillAsync("features");
        await Assertions.Expect(status).ToContainTextAsync("Unable to load navigation");

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator("#adminQuickNavigationModal")).ToBeHiddenAsync();
        await page.UnrouteAsync("**/QuickNavigation/Index**");
        await page.Locator("#adminQuickNavigationToggle").ClickAsync();
        await Assertions.Expect(page.Locator("#adminQuickNavigationResults [role=option]").First).ToBeVisibleAsync();
        await Assertions.Expect(status).ToBeHiddenAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_SourceResults_AreRenderedAsTextAndRefreshedOnReopen()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        var title = "<b>Custom destination</b>";
        await page.RouteAsync("**/QuickNavigation/Index**", route => route.FulfillAsync(new()
        {
            Json = new[]
            {
                new { source = "OptionalSource", id = "custom", title, path = new[] { "Custom" }, href = "/Admin", target = (string)null },
            },
        }));
        await page.Keyboard.PressAsync("Meta+k");

        var option = page.Locator("#adminQuickNavigationResults [role=option]").First;
        await Assertions.Expect(option.Locator(".admin-quick-navigation-title")).ToHaveTextAsync(title);
        await Assertions.Expect(option.Locator("b")).ToHaveCountAsync(0);

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator("#adminQuickNavigationModal")).ToBeHiddenAsync();
        title = "Updated destination";
        await page.Keyboard.PressAsync("Control+k");
        await Assertions.Expect(option.Locator(".admin-quick-navigation-title")).ToHaveTextAsync(title);

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_EscapeDuringOpening_ClosesPalette()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.EvaluateAsync("""
            () => {
                document.getElementById("adminQuickNavigationToggle").click();
                document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
            }
            """);

        await Assertions.Expect(page.Locator("body")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("modal-open"));
        await Assertions.Expect(page.Locator("#adminQuickNavigationModal")).ToBeHiddenAsync();
        await page.Locator("#adminQuickNavigationToggle").ClickAsync();
        await Assertions.Expect(page.Locator("#adminQuickNavigationInput")).ToBeFocusedAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickNavigation_ContentFeature_ConfiguresRecentItemsAndOpensEdit()
    {
        const string featureId = "OrchardCore.Contents.QuickNavigation";
        const string sourceName = "ContentItemNavigationSource";
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        var indexUrl = await page.Locator("#adminQuickNavigationModal").GetAttributeAsync("data-index-url");
        var before = await page.Context.APIRequest.GetAsync(indexUrl);
        Assert.DoesNotContain((await before.JsonAsync()).Value.EnumerateArray(),
            item => item.GetProperty("source").GetString() == sourceName);
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        var countInput = page.GetByLabel("Recent content items in quick navigation", new() { Exact = true });
        await Assertions.Expect(countInput).ToHaveCountAsync(0);

        await page.EnableFeatureAsync("", featureId);
        try
        {
            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await Assertions.Expect(countInput).ToHaveValueAsync("50");
            var contentHint = page.Locator(".ocat-wrapper").Filter(new() { Has = countInput }).Locator(".hint");
            var navigationHint = page.Locator(".ocat-wrapper")
                .Filter(new() { Has = page.GetByLabel("Enable quick navigation", new() { Exact = true }) }).Locator(".hint");
            var hintWidth = await contentHint.EvaluateAsync<double>("hint => hint.parentElement.getBoundingClientRect().width");
            Assert.Equal(await navigationHint.EvaluateAsync<double>("hint => hint.parentElement.getBoundingClientRect().width"), hintWidth);
            Assert.True(hintWidth > (await countInput.BoundingBoxAsync()).Width * 2);

            await countInput.FillAsync("2");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await Assertions.Expect(countInput).ToHaveValueAsync("2");

            var response = await page.Context.APIRequest.GetAsync(indexUrl);
            Assert.Equal(200, response.Status);
            var items = (await response.JsonAsync()).Value.EnumerateArray()
                .Where(item => item.GetProperty("source").GetString() == sourceName)
                .ToArray();
            Assert.Equal(2, items.Length);
            var title = items[0].GetProperty("title").GetString();
            var editUrl = items[0].GetProperty("href").GetString();
            Assert.EndsWith("/Edit", editUrl, StringComparison.Ordinal);

            await page.Keyboard.PressAsync("Control+k");
            await page.Locator("#adminQuickNavigationInput").FillAsync(title);
            await page.Locator($"#adminQuickNavigationResults a[href=\"{editUrl}\"]").ClickAsync();
            await page.WaitForURLAsync("**" + editUrl);

            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await countInput.FillAsync("1");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var updated = await page.Context.APIRequest.GetAsync(indexUrl);
            Assert.Equal(200, updated.Status);
            Assert.Single((await updated.JsonAsync()).Value.EnumerateArray(),
                item => item.GetProperty("source").GetString() == sourceName);

            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await countInput.FillAsync("0");
            await countInput.EvaluateAsync("input => input.form.noValidate = true");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
            await Assertions.Expect(page.Locator("[data-valmsg-for$='.MaxItems']")).ToContainTextAsync(
                "The number of recent content items must be at least 1.");
            await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
            await Assertions.Expect(countInput).ToHaveValueAsync("1");
        }
        finally
        {
            await page.DisableFeatureAsync("", featureId);
        }

        var after = await page.Context.APIRequest.GetAsync(indexUrl);
        Assert.DoesNotContain((await after.JsonAsync()).Value.EnumerateArray(),
            item => item.GetProperty("source").GetString() == sourceName);
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await Assertions.Expect(countInput).ToHaveCountAsync(0);
        await page.CloseAsync();
    }
}
