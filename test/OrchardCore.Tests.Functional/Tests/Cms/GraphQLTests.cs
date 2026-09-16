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

        await page.WaitForFunctionAsync(
            "() => window.monaco?.editor?.getModels().length > 0 && document.querySelector('.CodeMirror')?.CodeMirror");
        await page.EvaluateAsync(
            """
            values => {
                monaco.editor.getModels()[0].setValue(values.schema);
                document.querySelector(".CodeMirror").CodeMirror.setValue(values.query);
            }
            """,
            new
            {
                schema = """{ "type": "object", "properties": { "Value": { "type": "string" } } }""",
                query = "SELECT 'Allowed' AS Value",
            });

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
