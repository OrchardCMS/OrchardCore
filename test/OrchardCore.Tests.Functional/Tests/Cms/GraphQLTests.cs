using System.Text.Json;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class GraphQLTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    private const string Password = "Orchard1!";
    private const string QueryName = "PermissionTestQuery";
    private const string RoleName = "GraphQLQueryRunner";
    private const string UserName = "graphql-query-runner";

    public GraphQLTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    [Fact]
    public async Task NamedQuery_RequiresQueryPermission()
    {
        var prefix = $"/{Tenant.Prefix}";
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync(prefix);
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Apis.GraphQL");
        await CreateQueryAsync(page, prefix);

        await CreateRoleWithPermissionAsync(page, prefix, RoleName, "ExecuteGraphQL");
        await UserHelper.CreateUserAsync(page, prefix, UserName, "graphql-query-runner@test.com", Password, RoleName);
        await UserHelper.LoginAsAsync(page, prefix, UserName, Password);

        var response = await ExecuteQueryAsync(page, prefix);
        var content = await response.TextAsync();

        Assert.True(response.Status == 401, content);
        using (var result = JsonDocument.Parse(content))
        {
            Assert.Equal(
                "Unauthorized",
                result.RootElement.GetProperty("errors")[0].GetProperty("extensions").GetProperty("number").GetString());
        }

        await page.GotoAsync($"{prefix}/logout");
        await page.LoginAsync(prefix);
        await SetRolePermissionAsync(page, prefix, RoleName, $"ExecuteApi_{QueryName}");
        await UserHelper.LoginAsAsync(page, prefix, UserName, Password);

        response = await ExecuteQueryAsync(page, prefix);
        content = await response.TextAsync();

        Assert.True(response.Status == 200, content);
        using (var result = JsonDocument.Parse(content))
        {
            var item = Assert.Single(result.RootElement.GetProperty("data").GetProperty("permissionTestQuery").EnumerateArray());
            Assert.Equal("Allowed", item.GetProperty("value").GetString());
        }

        await page.CloseAsync();
    }

    private static async Task CreateQueryAsync(IPage page, string prefix)
    {
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Queries/Index");
        var createUrl = await page.Locator("a[href*='/Queries/Create']").GetAttributeAsync("href");
        Assert.NotNull(createUrl);
        await page.GotoAndAssertOkAsync(createUrl);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Name", Exact = true }).FillAsync(QueryName);

        // Drives both editors via real keyboard input (click + type) rather than reaching into
        // window.monaco/CodeMirror internals directly. That reach-in pattern broke once this
        // branch's ES-module refactor converted Query.Fields.Edit.cshtml's Schema field from the
        // old inline `require(['vs/editor/editor.main'], ...)` AMD bootstrap (which exposed a bare
        // `window.monaco` global) to the shared monaco-json-settings-editor bloom component, which
        // only exposes `window.__orchardCoreMonacoReady` (a Promise) - `window.monaco` is never a
        // global on this branch anymore, so the old wait/eval pattern could time out or silently
        // target the wrong (non-existent) API depending on load timing. Typing through the visible
        // editor surface, as MonacoLiquidIntelliSenseTests.cs already does for the same reason,
        // works regardless of what internals the editor happens to expose.
        var schemaEditor = page.Locator(".monaco-json-settings-editor-container .monaco-editor");
        await Assertions.Expect(schemaEditor).ToBeVisibleAsync();
        await schemaEditor.ClickAsync();
        await page.Keyboard.TypeAsync("""{ "type": "object", "properties": { "Value": { "type": "string" } } }""");

        var queryEditor = page.Locator(".codemirror-query-editor").Locator("xpath=following-sibling::div[contains(@class, 'CodeMirror')]");
        await Assertions.Expect(queryEditor).ToBeVisibleAsync();
        await queryEditor.ClickAsync();
        await page.Keyboard.TypeAsync("SELECT 'Allowed' AS Value");

        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<IAPIResponse> ExecuteQueryAsync(IPage page, string prefix)
        => await page.APIRequest.PostAsync(
            $"{Fixture.BaseUrl}{prefix}/api/graphql",
            new APIRequestContextOptions
            {
                DataObject = new
                {
                    query = "{ permissionTestQuery { value } }",
                },
            });

    private static async Task CreateRoleWithPermissionAsync(
        IPage page,
        string prefix,
        string roleName,
        string permissionName)
    {
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Roles/Create");
        await page.Locator("input[name='RoleName']").FillAsync(roleName);
        await page.ClickCreateAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SetRolePermissionAsync(page, prefix, roleName, permissionName);
    }

    private static async Task SetRolePermissionAsync(
        IPage page,
        string prefix,
        string roleName,
        string permissionName)
    {
        await page.GotoAndAssertOkAsync($"{prefix}/Admin/Roles/Edit/{Uri.EscapeDataString(roleName)}");

        var permission = page.Locator($"input[id='Checkbox.{permissionName}']");
        await permission.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        await permission.CheckAsync();

        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
