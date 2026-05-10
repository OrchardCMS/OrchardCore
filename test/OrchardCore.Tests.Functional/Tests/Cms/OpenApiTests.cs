using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class OpenApiTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public OpenApiTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    private async Task EnableOpenApiAsync(IPage page)
    {
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi");
    }

    private async Task EnableOpenApiWithSwaggerUIAsync(IPage page)
    {
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.SwaggerUI");
    }

    [Fact]
    public async Task CanEnableOpenApiFeature()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await Assertions.Expect(page.Locator("#btn-disable-OrchardCore_OpenApi")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task SwaggerUIIsAccessibleWhenEnabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);
        await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        await Assertions.Expect(page).ToHaveTitleAsync(new Regex("OrchardCore OpenAPI Documentation"));
        await page.CloseAsync();
    }

    [Fact]
    public async Task SwaggerJsonIsAccessibleWithoutAuthentication()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await page.CloseAsync();

        // New page without auth cookies.
        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.GotoAsync($"/{Tenant.Prefix}/swagger/v1/swagger.json");
        Assert.Equal(200, response.Status);
        var content = await anonPage.ContentAsync();
        Assert.Contains("\"openapi\"", content);
        await anonPage.CloseAsync();
    }

    [Fact]
    public async Task SwaggerUIReturns404WhenFeatureDisabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        // SwaggerUI feature is not enabled — should return 404.
        var response = await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        Assert.Equal(404, response.Status);
        await page.CloseAsync();
    }

    [Fact]
    public async Task UnauthenticatedAccessRedirectsToAdmin()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);
        await page.CloseAsync();

        var anonPage = await Fixture.CreatePageAsync();
        await anonPage.GotoAsync($"/{Tenant.Prefix}/swagger");
        Assert.Contains("/Login", anonPage.Url, StringComparison.OrdinalIgnoreCase);
        await anonPage.CloseAsync();
    }

    [Fact]
    public async Task UserWithoutApiManageCannotAccessSwaggerUI()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);

        // Create a user with the Editor role (no OpenAPI permissions).
        await UserHelper.CreateUserAsync(page, $"/{Tenant.Prefix}", "editor1", "editor1@test.com", "Orchard1!", "Editor");

        // Login as the editor.
        await UserHelper.LoginAsAsync(page, $"/{Tenant.Prefix}", "editor1", "Orchard1!");

        var response = await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        Assert.Equal(403, response.Status);
        await page.CloseAsync();
    }

    [Fact]
    public async Task UserWithoutApiManageCannotAccessSettings()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        // Create a user with the Editor role (no OpenAPI permissions).
        await UserHelper.CreateUserAsync(page, $"/{Tenant.Prefix}", "editor2", "editor2@test.com", "Orchard1!", "Editor");

        // Login as the editor.
        await UserHelper.LoginAsAsync(page, $"/{Tenant.Prefix}", "editor2", "Orchard1!");

        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");

        // The settings page should not render the OpenApi settings fields.
        await Assertions.Expect(page.Locator("#vue-AllowAnonymousSchemaAccess")).Not.ToBeAttachedAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task OpenApiSettingsPageIsAccessible()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await Assertions.Expect(page.Locator("#vue-AllowAnonymousSchemaAccess")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#vue-AuthenticationType")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task ReDocUIIsAccessibleWhenEnabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.ReDocUI");

        await page.GotoAsync($"/{Tenant.Prefix}/redoc");
        await Assertions.Expect(page).ToHaveTitleAsync(new Regex("OrchardCore OpenAPI Documentation"));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ScalarUIIsAccessibleWhenEnabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.ScalarUI");

        await page.GotoAsync($"/{Tenant.Prefix}/scalar/v1");
        await Assertions.Expect(page).ToHaveTitleAsync(new Regex("OrchardCore OpenAPI Documentation"));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ReDocReturns404WhenDisabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        // ReDocUI feature is not enabled — should return 404.
        var response = await page.GotoAsync($"/{Tenant.Prefix}/redoc");
        Assert.Equal(404, response.Status);
        await page.CloseAsync();
    }

    // --- Part 2: Cross-Tenant OAuth Tests ---

    /// <summary>
    /// Creates a new tenant with the Headless recipe (which includes OpenID Server features),
    /// configures the OpenID Server endpoints and flows, and returns the tenant info.
    /// </summary>
    private static async Task<TenantInfo> SetupAuthServerTenantAsync(IPage page)
    {
        var authTenant = TestUtils.GenerateTenantInfo("Headless", "OpenID Auth Server");

        // Login to the SaaS host and create the auth tenant.
        await AuthHelper.LoginAsync(page);
        await TenantHelper.CreateTenantAsync(page, authTenant);
        await TenantHelper.VisitTenantSetupPageAsync(page, authTenant);
        await TenantHelper.SiteSetupAsync(page, authTenant);

        // Login to the auth tenant — use the SaaS admin credentials since we created
        // the tenant with the same username/password.
        await page.GotoAsync($"/{authTenant.Prefix}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If the login page is shown, fill in credentials.
        if (page.Url.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            var config = TestUtils.DefaultConfig;
            await page.Locator("#LoginForm_UserName").FillAsync(config.Username);
            await page.Locator("#LoginForm_Password").FillAsync(config.Password);
            await page.Locator("button[type='submit']").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Navigate to the OpenID Server Configuration page.
        // First, ensure we are authenticated on the auth tenant's admin.
        await page.GotoAsync($"/{authTenant.Prefix}/Admin");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If redirected to login, authenticate.
        if (page.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase)
            || page.Url.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            var config = TestUtils.DefaultConfig;
            await page.Locator("#LoginForm_UserName").FillAsync(config.Username);
            await page.Locator("#LoginForm_Password").FillAsync(config.Password);
            await page.Locator("button[type='submit']").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        var configUrl = $"/{authTenant.Prefix}/Admin/OpenId/ServerConfiguration";
        await page.GotoAsync(configUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The OpenID Server settings form uses a display driver prefix on field IDs.
        // Use label-based locators to find checkboxes reliably.
        var enableTokenEndpoint = page.Locator("label:has-text('Enable Token Endpoint')").Locator("..").Locator("input[type='checkbox']");
        await enableTokenEndpoint.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 15000 });
        if (!await enableTokenEndpoint.IsCheckedAsync())
        {
            await enableTokenEndpoint.CheckAsync();
        }

        var enableAuthEndpoint = page.Locator("label:has-text('Enable Authorization Endpoint')").Locator("..").Locator("input[type='checkbox']");
        if (!await enableAuthEndpoint.IsCheckedAsync())
        {
            await enableAuthEndpoint.CheckAsync();
        }

        // Wait for flow checkboxes to appear (Bootstrap collapse animation).
        var allowClientCreds = page.Locator("label:has-text('Allow Client Credentials Flow')").Locator("..").Locator("input[type='checkbox']");
        await allowClientCreds.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        if (!await allowClientCreds.IsCheckedAsync())
        {
            await allowClientCreds.CheckAsync();
        }

        var allowAuthCode = page.Locator("label:has-text('Allow Authorization Code Flow')").Locator("..").Locator("input[type='checkbox']");
        await allowAuthCode.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        if (!await allowAuthCode.IsCheckedAsync())
        {
            await allowAuthCode.CheckAsync();
        }

        // Submit the server config form.
        await page.Locator("button.btn-primary[type='submit']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return authTenant;
    }

    /// <summary>
    /// Creates an OpenID Connect application on the auth server tenant.
    /// </summary>
#pragma warning disable IDE0051 // Remove unused private members — will be used in upcoming OAuth tests
    private static async Task CreateOpenIdApplicationAsync(IPage page, string authPrefix, string clientId, string clientSecret)
#pragma warning restore IDE0051
    {
        await page.GotoAsync($"/{authPrefix}/Admin/OpenId/Application/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The Application Create form uses asp-for with IDs like DisplayName, ClientId, etc.
        // These are direct form fields (not display driver shapes), so IDs should be direct.
        await page.Locator("input[name='DisplayName']").FillAsync("OpenApi Test Client");
        await page.Locator("input[name='ClientId']").FillAsync(clientId);
        await page.Locator("input[name='ClientSecret']").FillAsync(clientSecret);

        // Show and check the Client Credentials flow checkbox.
        var clientCredsFieldset = page.Locator("#AllowClientCredentialsFlowFieldSet");
        await clientCredsFieldset.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        var allowClientCreds = page.Locator("#AllowClientCredentialsFlow");
        if (!await allowClientCreds.IsCheckedAsync())
        {
            await allowClientCreds.CheckAsync();
        }

        // Wait for the Role Group to appear and assign Administrator role.
        var roleGroup = page.Locator("#RoleGroup");
        await roleGroup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Find the Administrator role checkbox within the role group.
        var adminRoleCheckbox = roleGroup.Locator("input[type='checkbox']").First;
        if (!await adminRoleCheckbox.IsCheckedAsync())
        {
            await adminRoleCheckbox.CheckAsync();
        }

        await page.Locator(".btn.save").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task SettingsValidateDiscoveryDocumentFromCrossTenantAuthServer()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        var authTenant = await SetupAuthServerTenantAsync(page);

        // Navigate back to the main tenant and login.
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        // Select Client Credentials auth type (value = 2).
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("2");

        // Fill in Token URL pointing to the auth server tenant.
        var tokenUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/token";
        await page.Locator("#vue-TokenUrl").FillAsync(tokenUrl);
        await page.Locator("#vue-OAuthClientId").FillAsync("openapi-test");

        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify no validation errors — success message should appear.
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task SettingsShowErrorForInvalidAuthServerUrl()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        // Select Client Credentials auth type.
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("2");

        // Fill in a bogus Token URL.
        var tokenUrl = $"{Fixture.BaseUrl}/nonexistent-tenant/connect/token";
        await page.Locator("#vue-TokenUrl").FillAsync(tokenUrl);
        await page.Locator("#vue-OAuthClientId").FillAsync("test");

        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify validation error is displayed.
        await Assertions.Expect(page.Locator(".validation-summary-errors")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task SettingsValidatePkceFromCrossTenantAuthServer()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        var authTenant = await SetupAuthServerTenantAsync(page);

        // Navigate back to the main tenant and login.
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        // Select Authorization Code + PKCE auth type (value = 1).
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("1");

        // Fill in Authorization URL and Token URL pointing to the auth server tenant.
        var authorizationUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/authorize";
        var tokenUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/token";
        await page.Locator("#vue-AuthorizationUrl").FillAsync(authorizationUrl);
        await page.Locator("#vue-TokenUrl").FillAsync(tokenUrl);
        await page.Locator("#vue-OAuthClientId").FillAsync("openapi-pkce");

        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify no validation errors — success message should appear.
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConnectionTesterUIAppearsWithCrossTenantClientCredentials()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        var clientId = "openapi-conn-test";

        var authTenant = await SetupAuthServerTenantAsync(page);

        // Navigate back to the main tenant and configure OpenApi settings.
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");

        // Wait for the Vue app to mount before interacting with its elements.
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        // Select Client Credentials auth type.
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("2");

        var tokenUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/token";
        await page.Locator("#vue-TokenUrl").FillAsync(tokenUrl);
        await page.Locator("#vue-OAuthClientId").FillAsync(clientId);

        // Save settings first (required for the connection tester to appear).
        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate back to settings page to verify the connection tester UI appears.
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");

        // Verify the ConnectionTester component rendered: client secret field and button visible.
        var clientSecretInput = page.Locator("#vue-ClientSecret");
        await clientSecretInput.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await Assertions.Expect(clientSecretInput).ToBeVisibleAsync();

        var testButton = page.Locator("button:has-text('Test Connection')");
        await Assertions.Expect(testButton).ToBeVisibleAsync();

        // Verify the button is disabled until a secret is provided (canTest computed is false).
        await Assertions.Expect(testButton).ToBeDisabledAsync();

        // Fill in the secret — button should become enabled.
        await clientSecretInput.FillAsync("some-secret");
        await Assertions.Expect(testButton).ToBeEnabledAsync();

        await page.CloseAsync();
    }
}
