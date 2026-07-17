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
    /// Creates an OpenID Connect application configured for the Authorization Code + PKCE
    /// flow, with consent set to "implicit" so the automated flow never has to click through
    /// a consent screen.
    /// </summary>
    private static async Task CreateOpenIdPkceApplicationAsync(IPage page, string authPrefix, string clientId, string clientSecret, string redirectUri)
    {
        await page.GotoAsync($"/{authPrefix}/Admin/OpenId/Application/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.Locator("input[name='DisplayName']").FillAsync("OpenApi PKCE Test Client");
        await page.Locator("input[name='ClientId']").FillAsync(clientId);
        await page.Locator("input[name='ClientSecret']").FillAsync(clientSecret);

        var authCodeFieldset = page.Locator("#AllowAuthorizationCodeFlowFieldSet");
        await authCodeFieldset.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        var allowAuthCode = page.Locator("#AllowAuthorizationCodeFlow");
        if (!await allowAuthCode.IsCheckedAsync())
        {
            await allowAuthCode.CheckAsync();
        }

        var redirectSection = page.Locator("#RedirectSection");
        await redirectSection.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.Locator("input[name='RedirectUris']").FillAsync(redirectUri);

        var requirePkce = page.Locator("#RequireProofKeyForCodeExchange");
        if (!await requirePkce.IsCheckedAsync())
        {
            await requirePkce.CheckAsync();
        }

        // Skip the consent screen so the automated flow never has to click through it.
        await page.Locator("select[name='ConsentType']").SelectOptionAsync("implicit");

        // No client role assignment here: an Authorization Code token carries the signed-in
        // user's own roles, unlike Client Credentials where the app has no user to inherit from.
        await page.Locator(".btn.save").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task SettingsShowErrorForInvalidAuthServerUrl()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        // Select Authorization Code + PKCE auth type.
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("1");

        // Fill in a bogus Authorization URL and Token URL.
        var authorizationUrl = $"{Fixture.BaseUrl}/nonexistent-tenant/connect/authorize";
        var tokenUrl = $"{Fixture.BaseUrl}/nonexistent-tenant/connect/token";
        await page.Locator("#vue-AuthorizationUrl").FillAsync(authorizationUrl);
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
    public async Task ConnectionTesterUIAppearsWithCrossTenantPkce()
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

        // Select Authorization Code + PKCE auth type.
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("1");

        var authorizationUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/authorize";
        var tokenUrl = $"{Fixture.BaseUrl}/{authTenant.Prefix}/connect/token";
        await page.Locator("#vue-AuthorizationUrl").FillAsync(authorizationUrl);
        await page.Locator("#vue-TokenUrl").FillAsync(tokenUrl);
        await page.Locator("#vue-OAuthClientId").FillAsync(clientId);

        // Save settings first (required for the connection tester to appear).
        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate back to settings page to verify the connection tester UI appears.
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");

        // Verify the ConnectionTester component rendered: no client secret field (PKCE is a
        // public-client flow), and the button is enabled since all required fields are set.
        await Assertions.Expect(page.Locator("#vue-ClientSecret")).Not.ToBeAttachedAsync();

        var testButton = page.Locator("button:has-text('Test Connection')");
        await Assertions.Expect(testButton).ToBeVisibleAsync();
        await Assertions.Expect(testButton).ToBeEnabledAsync();

        await page.CloseAsync();
    }

    private static async Task<string> GetAntiForgeryTokenAsync(IPage page, string prefix)
    {
        await page.GotoAsync($"{prefix}/Admin/Settings/openapi");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return await page.Locator("input[name='__RequestVerificationToken']").First.InputValueAsync();
    }

    private static async Task CreateRoleWithPermissionAsync(IPage page, string prefix, string roleName, string permissionName)
    {
        await page.GotoAsync($"{prefix}/Admin/Roles/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("input[name='RoleName']").FillAsync(roleName);
        await page.ClickCreateAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"{prefix}/Admin/Roles/Edit/{roleName}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var checkbox = page.Locator($"input[id='Checkbox.{permissionName}']");
        await checkbox.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 10000 });
        if (!await checkbox.IsCheckedAsync())
        {
            await checkbox.CheckAsync();
        }

        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Regression test for a permission-hierarchy bug where ManageOpenApi was (incorrectly)
    /// implied by ViewOpenApiContent, letting a view-only role edit settings and call the
    /// test-connection endpoint. A view-only role must be able to view the documentation UIs
    /// (per the module's documented design) but must never be treated as ManageOpenApi.
    /// </summary>
    [Fact]
    public async Task UserWithOnlyViewOpenApiContentCanViewButCannotManage()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);

        await CreateRoleWithPermissionAsync(page, $"/{Tenant.Prefix}", "OpenApiViewer", "ViewOpenApiContent");
        await UserHelper.CreateUserAsync(page, $"/{Tenant.Prefix}", "viewer1", "viewer1@test.com", "Orchard1!", "OpenApiViewer");
        await UserHelper.LoginAsAsync(page, $"/{Tenant.Prefix}", "viewer1", "Orchard1!");

        // Viewing the documentation UI is allowed with just ViewOpenApiContent.
        var swaggerResponse = await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        Assert.Equal(200, swaggerResponse.Status);

        // Managing settings must still require ManageOpenApi — the settings fields must stay hidden.
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await Assertions.Expect(page.Locator("#vue-AllowAnonymousSchemaAccess")).Not.ToBeAttachedAsync();

        await page.CloseAsync();
    }

    /// <summary>
    /// Regression test for an SSRF guard added to the test-connection endpoint: a TokenUrl
    /// pointing at a link-local address (e.g. a cloud metadata endpoint) must be rejected
    /// before the server makes any outbound request to it.
    /// </summary>
    [Fact]
    public async Task TestConnectionRejectsPrivateNetworkTokenUrl()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        var antiForgeryToken = await GetAntiForgeryTokenAsync(page, $"/{Tenant.Prefix}");

        var response = await page.APIRequest.PostAsync(
            $"{Fixture.BaseUrl}/{Tenant.Prefix}/api/openapi/test-connection",
            new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string> { ["RequestVerificationToken"] = antiForgeryToken },
                DataObject = new
                {
                    authenticationType = 1,
                    tokenUrl = "http://169.254.169.254/latest/meta-data/",
                    clientId = "test",
                },
            });

        Assert.Equal(400, response.Status);

        var body = await response.TextAsync();
        Assert.Contains("not allowed", body, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    /// <summary>
    /// Regression test for a CSRF fix: the test-connection endpoint now accepts ambient cookie
    /// authentication (so the settings page's own "Test Connection" button can call it without
    /// a bearer token). A cookie-authenticated request with no matching antiforgery token — the
    /// only thing a cross-site attacker riding a victim's session cookie could produce, since
    /// the token itself is never exposed to another origin — must be rejected.
    /// </summary>
    [Fact]
    public async Task TestConnectionRejectsCookieAuthenticatedRequestWithoutAntiforgeryToken()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        var response = await page.APIRequest.PostAsync(
            $"{Fixture.BaseUrl}/{Tenant.Prefix}/api/openapi/test-connection",
            new APIRequestContextOptions
            {
                DataObject = new
                {
                    authenticationType = 1,
                    tokenUrl = "https://example.com/connect/token",
                    clientId = "test",
                },
            });

        Assert.Equal(400, response.Status);

        await page.CloseAsync();
    }

    // --- Part 3: Live auth-flow tests through the real Swagger UI / Scalar UI widgets ---
    //
    // These configure a same-tenant OpenID Connect Server (so the token is issued and
    // validated by the same tenant that hosts the OpenApi-protected endpoint being called),
    // drive each documentation UI's own OAuth widget to obtain a real token, then execute
    // that UI's own "try it out" / "test request" action against a real protected endpoint
    // and assert the request was authenticated (not a 401/403), rather than just checking
    // that settings save or that the UI is reachable.

    private static async Task ConfigureOpenIdServerForAuthorizationCodeAsync(IPage page, string prefix)
    {
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.OpenId.Server");
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.OpenId.Validation");

        await page.GotoAsync($"{prefix}/Admin/OpenId/ServerConfiguration");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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

        var allowAuthCode = page.Locator("label:has-text('Allow Authorization Code Flow')").Locator("..").Locator("input[type='checkbox']");
        await allowAuthCode.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        if (!await allowAuthCode.IsCheckedAsync())
        {
            await allowAuthCode.CheckAsync();
        }

        await page.Locator("button.btn-primary[type='submit']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task SwaggerUIAuthorizesAndCallsProtectedEndpointWithPkce()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);

        var clientId = "openapi-swagger-pkce";
        var clientSecret = "swagger-pkce-secret";
        var redirectUri = $"{Fixture.BaseUrl}/{Tenant.Prefix}/swagger/oauth2-redirect.html";

        await ConfigureOpenIdServerForAuthorizationCodeAsync(page, $"/{Tenant.Prefix}");
        await CreateOpenIdPkceApplicationAsync(page, Tenant.Prefix, clientId, clientSecret, redirectUri);

        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("1");
        await page.Locator("#vue-AuthorizationUrl").FillAsync($"{Fixture.BaseUrl}/{Tenant.Prefix}/connect/authorize");
        await page.Locator("#vue-TokenUrl").FillAsync($"{Fixture.BaseUrl}/{Tenant.Prefix}/connect/token");
        await page.Locator("#vue-OAuthClientId").FillAsync(clientId);
        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.Locator(".btn.authorize").First.ClickAsync();
        await page.Locator("#client_id_authorizationCode").FillAsync(clientId);
        await page.Locator("#client_secret_authorizationCode").FillAsync(clientSecret);

        // The Authorization Code + PKCE flow opens a popup to /connect/authorize. The admin
        // is already logged into this tenant, and the app's ConsentType is "implicit", so the
        // popup redirects straight back to Swagger's oauth2-redirect.html and self-closes.
        await page.RunAndWaitForPopupAsync(
            async () => await page.Locator(".auth-btn-wrapper button.authorize").ClickAsync());

        // The popup closes itself once the redirect completes; give the main page time to
        // finish the token exchange it triggers.
        await page.WaitForTimeoutAsync(3000);

        await Assertions.Expect(page.Locator(".auth-btn-wrapper button.authorize")).ToHaveTextAsync("Logout");
        await page.Locator(".auth-btn-wrapper button.btn-done").ClickAsync();

        var operation = page.Locator("#operations-OpenApiApi-ApiTestOpenApiConnection");
        await operation.Locator(".opblock-summary").ClickAsync();
        await operation.Locator("button.try-out__btn").ClickAsync();

        var response = await page.RunAndWaitForResponseAsync(
            async () => await operation.Locator("button.execute").ClickAsync(),
            r => r.Url.Contains("/api/openapi/test-connection"));

        Assert.NotEqual(401, response.Status);
        Assert.NotEqual(403, response.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ScalarUIRendersOperationsUnderTenantPrefix()
    {
        // Regression test: Scalar's own client script re-derives the tenant's URL prefix
        // from window.location and prepends it to the configured OpenAPI route pattern.
        // Passing an already-prefixed path (as Swagger UI/ReDoc do) makes the prefix apply
        // twice, 404s the spec fetch, and leaves the sidebar empty — invisible to a test that
        // only checks the page title, which is why it slipped through before.
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.ScalarUI");

        var response = await page.RunAndWaitForResponseAsync(
            async () =>
            {
                await page.GotoAsync($"/{Tenant.Prefix}/scalar/v1");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            },
            r => r.Url.Contains("/swagger/v1/swagger.json"));

        Assert.Equal(200, response.Status);
        await Assertions.Expect(page.Locator(".sidebar").First).ToContainTextAsync("OpenApiApi");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ScalarUIAuthorizesAndCallsProtectedEndpointWithPkce()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.ScalarUI");

        var clientId = "openapi-scalar-pkce";
        var clientSecret = "scalar-pkce-secret";
        var redirectUri = $"{Fixture.BaseUrl}/{Tenant.Prefix}/scalar/v1";

        await ConfigureOpenIdServerForAuthorizationCodeAsync(page, $"/{Tenant.Prefix}");
        await CreateOpenIdPkceApplicationAsync(page, Tenant.Prefix, clientId, clientSecret, redirectUri);

        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#vue-AuthenticationType").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.Locator("#vue-AuthenticationType").SelectOptionAsync("1");
        await page.Locator("#vue-AuthorizationUrl").FillAsync($"{Fixture.BaseUrl}/{Tenant.Prefix}/connect/authorize");
        await page.Locator("#vue-TokenUrl").FillAsync($"{Fixture.BaseUrl}/{Tenant.Prefix}/connect/token");
        await page.Locator("#vue-OAuthClientId").FillAsync(clientId);
        await ButtonHelper.ClickSaveAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"/{Tenant.Prefix}/scalar/v1");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(1000);

        await page.Locator("input[placeholder='XYZ123']").First.FillAsync(clientSecret);

        // "Use PKCE" is a headless-ui menu button, not a native <select>, and its "yes" value
        // (actually the challenge method name, e.g. "SHA-256") isn't unique page-wide — other
        // boolean parameters render the same kind of menu. Open the one scoped to the "Use
        // PKCE" row and move off "no" with the keyboard instead of matching option text.
        await page.Locator("tr:has-text('Use PKCE') button").First.ClickAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("Enter");

        // The Authorization Code + PKCE flow opens a popup to /connect/authorize. The admin
        // is already logged into this tenant, and the app's ConsentType is "implicit", so the
        // popup redirects straight back to the Scalar page (the configured redirect URI) and
        // self-closes once it has relayed the code back to the opener.
        await page.RunAndWaitForPopupAsync(
            async () => await page.Locator("button:has-text('Authorize')").First.ClickAsync());

        // The popup closes itself once the redirect completes; give the main page time to
        // finish the token exchange it triggers.
        await page.WaitForTimeoutAsync(3000);

        await Assertions.Expect(page.Locator("tr:has-text('Access Token') input").First).Not.ToHaveValueAsync(string.Empty);

        await page.Locator(".sidebar a", new() { HasText = "OpenApiApi" }).First.ClickAsync();
        var operation = page.Locator("[id='tag/openapiapi/POST/api/openapi/test-connection']");
        await operation.Locator("button.show-api-client-button").ClickAsync();

        var response = await page.RunAndWaitForResponseAsync(
            async () => await page.GetByRole(AriaRole.Button, new() { Name = "Send Request", Exact = true }).ClickAsync(),
            r => r.Url.Contains("/api/openapi/test-connection"));

        Assert.NotEqual(401, response.Status);
        Assert.NotEqual(403, response.Status);

        await page.CloseAsync();
    }
}
