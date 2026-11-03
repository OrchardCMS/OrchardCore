using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AssistedTemplatesActionsTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    // The templates of the theme for the shape of the content items, by content type.
    private const string ThemeTemplates = "/Admin/TemplateSources/Theme?Options.Search=Content__";

    public AssistedTemplatesActionsTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    private string Prefix => $"/{Tenant.Prefix}";

    [Fact]
    public async Task BulkActions_ClonesAndTemplatesWrittenByHand_OnlyChangeWhatTheyApplyTo()
    {
        var page = await StartAsync("OrchardCore.Templates.Assisted");

        // The list keeps its search after the action.
        await page.GotoAndAssertOkAsync($"{Prefix}{ThemeTemplates}");
        await ApplyBulkActionAsync(page, "Clone", "Content__Article", "Content__Blog");
        await Assertions.Expect(page.Locator(".toast-body")).ToContainTextAsync("2 template(s) cloned.");
        Assert.Contains("Options.Search=Content__", page.Url, StringComparison.Ordinal);

        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Edit/Content__Article", "Edited");
        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Create?name=Content__Category", "Written by hand");

        await page.GotoAndAssertOkAsync($"{Prefix}{ThemeTemplates}");
        await AssertStatusAsync(page, "Content__Article", "Edited");
        await AssertStatusAsync(page, "Content__Blog", "Cloned");
        await AssertStatusAsync(page, "Content__Category", "Overridden");

        // The reset only applies to the clones: a template written by hand has its own button, with its own warning.
        await ApplyBulkActionAsync(page, "Reset", "Content__Article", "Content__Blog", "Content__Category");
        await Assertions.Expect(page.Locator(".toast-body")).ToContainTextAsync("2 template(s) reset to their source.");
        await AssertStatusAsync(page, "Content__Article", "Cloned");
        await AssertStatusAsync(page, "Content__Blog", "Cloned");
        await AssertStatusAsync(page, "Content__Category", "Overridden");

        await page.GotoAndAssertOkAsync($"{Prefix}/Admin/Templates/Edit/Content__Category");
        Assert.Equal("Written by hand", await page.Locator("#Content").InputValueAsync());

        await page.GotoAndAssertOkAsync($"{Prefix}{ThemeTemplates}");
        await ApplyBulkActionAsync(page, "Remove", "Content__Article", "Content__Category");
        await Assertions.Expect(page.Locator(".toast-body")).ToContainTextAsync("2 template override(s) deleted.");
        await AssertStatusAsync(page, "Content__Article", "Not overridden");
        await AssertStatusAsync(page, "Content__Blog", "Cloned");
        await AssertStatusAsync(page, "Content__Category", "Not overridden");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ClonedTemplate_DeletedOrRenamedWithFeatureDisabled_IsNotMonitoredOnceWrittenByHand()
    {
        var page = await StartAsync("OrchardCore.Templates.Assisted");

        await page.GotoAndAssertOkAsync($"{Prefix}{ThemeTemplates}");
        await ApplyBulkActionAsync(page, "Clone", "Content__Article", "Content__Blog");
        await AssertStatusAsync(page, "Content__Article", "Cloned");
        await AssertStatusAsync(page, "Content__Blog", "Cloned");

        await FeatureHelper.DisableFeatureAsync(page, Prefix, "OrchardCore.Templates.Assisted");

        // The templates list deletes one, and the editor renames the other one.
        await page.GotoAndAssertOkAsync($"{Prefix}/Admin/Templates");
        await page.Locator(".list-group-item", new() { HasText = "Content__Article" }).Locator("a.btn-danger").ClickAsync();
        await page.ClickModalOkAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync($"{Prefix}/Admin/Templates/Edit/Content__Blog");
        await page.Locator("#Name").FillAsync("Content__BlogRenamed");
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Create?name=Content__Article", "Written by hand");
        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Create?name=Content__Blog", "Written by hand");

        await FeatureHelper.EnableFeatureAsync(page, Prefix, "OrchardCore.Templates.Assisted");

        await page.GotoAndAssertOkAsync($"{Prefix}{ThemeTemplates}");
        await AssertStatusAsync(page, "Content__Article", "Overridden");
        await AssertStatusAsync(page, "Content__Blog", "Overridden");

        await page.CloseAsync();
    }

    [Fact]
    public async Task InspectContent_LocalTemplates_ListsTheShapesOfTheFrontEndTemplates()
    {
        var page = await StartAsync("OrchardCore.Templates.Assisted", "OrchardCore.Templates.ShapeTracing", "OrchardCore.AdminTemplates");

        var search = await page.APIRequest.GetAsync($"{Prefix}/Admin/TemplateInspector/SearchContentItems?query=About");
        var contentItemId = (await search.JsonAsync())?.EnumerateArray().First().GetProperty("id").GetString();
        var inspector = $"{Prefix}/Admin/TemplateInspector?contentItemId={contentItemId}";

        var tree = page.Locator("ul.list-group.mb-3");
        var bodyPart = tree.Locator("code.fs-6").GetByText("HtmlBodyPart", new() { Exact = true });

        // An admin template doesn't change what the front end renders, although the page is an admin one.
        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Create?name=Content&adminTemplates=true", "Admin template");
        await page.GotoAndAssertOkAsync(inspector);
        await Assertions.Expect(bodyPart).ToHaveCountAsync(1);
        await Assertions.Expect(tree).ToContainTextAsync("TheBlogTheme");

        // The template of the theme rendering the shape is cloned from there, and goes on rendering the same shapes.
        await tree.Locator("> li").First.Locator("a.btn", new() { HasText = "Clone" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.Locator(".toast-body")).ToContainTextAsync("Content__Article");
        await Assertions.Expect(tree).ToContainTextAsync("Local template Content__Article");
        await Assertions.Expect(bodyPart).ToHaveCountAsync(1);

        // A local template not rendering its children doesn't list them.
        await SaveTemplateAsync(page, $"{Prefix}/Admin/Templates/Edit/Content__Article", "Without its children");
        await page.GotoAndAssertOkAsync(inspector);
        await Assertions.Expect(tree).ToContainTextAsync("Local template Content__Article");
        await Assertions.Expect(bodyPart).ToHaveCountAsync(0);

        // The admin renders the content item with its own templates, and the admin theme.
        await page.GotoAndAssertOkAsync($"{inspector}&adminTemplates=true");
        await Assertions.Expect(tree).ToBeVisibleAsync();
        await Assertions.Expect(tree).Not.ToContainTextAsync("Local template Content__Article");

        await page.CloseAsync();
    }

    private async Task<IPage> StartAsync(params string[] features)
    {
        var page = await Fixture.CreatePageAsync();

        await AuthHelper.LoginAsync(page, Prefix);

        foreach (var feature in features)
        {
            await FeatureHelper.EnableFeatureAsync(page, Prefix, feature);
        }

        return page;
    }

    // Selects templates of the current list and applies a bulk action to them.
    private static async Task ApplyBulkActionAsync(IPage page, string action, params string[] names)
    {
        foreach (var name in names)
        {
            await page.Locator($"input[id='itemIds-{name}']").CheckAsync();
        }

        await page.Locator("#bulk-action-menu-button").ClickAsync();
        await page.Locator($"#actions [data-action={action}]").ClickAsync();
        await page.ClickModalOkAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static Task AssertStatusAsync(IPage page, string name, string status)
        => Assertions.Expect(page.Locator(".list-group-item", new() { Has = page.Locator($"input[id='itemIds-{name}']") }).Locator(".badge", new() { HasText = status }).Last)
            .ToHaveTextAsync(status);

    // Opens the editor of a template, replaces its content and saves it.
    private static async Task SaveTemplateAsync(IPage page, string url, string content)
    {
        await page.GotoAndAssertOkAsync(url);
        await page.Locator("#Content_editor .monaco-editor").ClickAsync();
        await page.Keyboard.PressAsync("ControlOrMeta+a");
        await page.Keyboard.InsertTextAsync(content);
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
