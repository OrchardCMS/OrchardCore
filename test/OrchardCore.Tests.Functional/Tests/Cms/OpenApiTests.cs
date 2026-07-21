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
    public async Task SwaggerJsonRequiresAuthenticationUntilAnonymousAccessIsEnabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        // Anonymous schema access is disabled by default: a page without auth
        // cookies must get a 401 for the JSON schema endpoint. The 401 is issued
        // through the "Api" scheme, so it must advertise a challenge
        // (RFC 9110 §15.5.2) — the Bearer fallback when OpenID validation is off.
        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.GotoAsync($"/{Tenant.Prefix}/swagger/v1/swagger.json");
        Assert.Equal(401, response.Status);
        Assert.Contains("Bearer", (await response.AllHeadersAsync())["www-authenticate"]);

        // Opt in to anonymous schema access via the settings UI.
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#AllowAnonymousSchemaAccess").CheckAsync();
        await ButtonHelper.ClickSaveAsync(page);
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync();
        await page.CloseAsync();

        // The same anonymous page can now fetch the schema.
        response = await anonPage.GotoAsync($"/{Tenant.Prefix}/swagger/v1/swagger.json");
        Assert.Equal(200, response.Status);
        var content = await anonPage.ContentAsync();
        Assert.Contains("\"openapi\"", content);
        await anonPage.CloseAsync();
    }

    /// <summary>
    /// Covers the recipe delivery path for anonymous schema access, the mechanism NSwag
    /// generation depends on: running the OpenApiGeneration recipe must both enable the
    /// OpenApi feature and turn on AllowAnonymousSchemaAccess, and the change must be
    /// live (shell released) once the recipe reports success. Runs on the Default tenant
    /// because the recipe enables OrchardCore.Tenants, which is Default-tenant-only.
    /// </summary>
    [Fact]
    public async Task OpenApiGenerationRecipeEnablesAnonymousSchemaAccess()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page);

        await page.GotoAsync("/Admin/Recipes");
        await page.Locator("#btn-run-OpenApiGeneration").ClickAsync();
        await page.ClickModalOkAsync();
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync(new() { Timeout = 60000 });
        await page.CloseAsync();

        // The schema must be anonymously fetchable without any further action.
        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.GotoAsync("/swagger/v1/swagger.json");
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
        await Assertions.Expect(page.Locator("#AllowAnonymousSchemaAccess")).Not.ToBeAttachedAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task OpenApiSettingsPageIsAccessible()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);

        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await Assertions.Expect(page.Locator("#AllowAnonymousSchemaAccess")).ToBeVisibleAsync();
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
    /// implied by ViewOpenApiContent, letting a view-only role edit settings. A view-only role
    /// must be able to view the documentation UIs (per the module's documented design) but must
    /// never be treated as ManageOpenApi.
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
        await Assertions.Expect(page.Locator("#AllowAnonymousSchemaAccess")).Not.ToBeAttachedAsync();

        await page.CloseAsync();
    }

    // --- Live auth-flow tests through the real Swagger UI / Scalar UI widgets ---
    //
    // These configure a same-tenant OpenID Connect Server (so the token is issued and validated
    // by the same tenant that hosts the OpenApi-protected endpoint being called) and register the
    // public "openapi" PKCE client the documentation UIs use. The injected openapi-ui-auth bundle
    // then silently acquires a bearer token from the already-authenticated admin session (no
    // "Authorize" click), and each UI's "try it out" / "test request" action against a real
    // protected endpoint is asserted to be authenticated (not a 401/403).

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

    /// <summary>
    /// Registers the public (secret-less) "openapi" PKCE client the documentation UIs use, with
    /// the silent-renew page as its redirect URI and the roles scope granted (so the issued token
    /// carries the user's roles, which the API permission checks require). Consent is "implicit"
    /// so the silent prompt=none flow never has to render a consent screen. This mirrors the
    /// OpenApiPkce recipe, but with the redirect URI resolved to the running tenant's real origin.
    /// </summary>
    private static async Task RegisterOpenApiSilentClientAsync(IPage page, string prefix, string redirectUri)
    {
        await page.GotoAsync($"{prefix}/Admin/OpenId/Application/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.Locator("input[name='DisplayName']").FillAsync("OpenAPI Interactive UI");
        await page.Locator("input[name='ClientId']").FillAsync("openapi");

        // Public client: no secret, browser-driven PKCE.
        await page.Locator("#Type").SelectOptionAsync("public");

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

        // Skip the consent screen so the silent prompt=none flow never has to click through it.
        await page.Locator("select[name='ConsentType']").SelectOptionAsync("implicit");

        // Grant the scopes the documentation UIs request; roles is what carries the caller's roles
        // into the access token. The scope checkboxes are labelled by scope name.
        foreach (var scope in new[] { "email", "profile", "roles" })
        {
            var scopeCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = scope, Exact = true });
            if (await scopeCheckbox.CountAsync() > 0 && !await scopeCheckbox.First.IsCheckedAsync())
            {
                await scopeCheckbox.First.CheckAsync();
            }
        }

        await page.Locator(".btn.save").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// With OpenID token validation enabled, an anonymous schema fetch must carry the full
    /// RFC-compliant challenge: 401 status, a WWW-Authenticate header, and the RFC 9457
    /// Problem Details body attached by the validation feature's challenge handler —
    /// matching the responses of the API endpoints the schema describes.
    /// </summary>
    [Fact]
    public async Task SwaggerJsonChallengeCarriesProblemDetailsWhenValidationEnabled()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await ConfigureOpenIdServerForAuthorizationCodeAsync(page, $"/{Tenant.Prefix}");

        // Force anonymous schema access off so this test does not depend on whether
        // another test has enabled it on this shared tenant.
        await page.GotoAsync($"/{Tenant.Prefix}/Admin/Settings/openapi");
        await page.Locator("#AllowAnonymousSchemaAccess").UncheckAsync();
        await ButtonHelper.ClickSaveAsync(page);
        await Assertions.Expect(page.Locator(".message-success")).ToBeVisibleAsync();
        await page.CloseAsync();

        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.GotoAsync($"/{Tenant.Prefix}/swagger/v1/swagger.json");
        Assert.Equal(401, response.Status);

        var headers = await response.AllHeadersAsync();
        Assert.Contains("Bearer", headers["www-authenticate"]);
        Assert.Contains("application/problem+json", headers["content-type"]);

        var body = await response.TextAsync();
        Assert.Contains("\"status\":401", body);
        Assert.Contains("\"title\"", body);
        await anonPage.CloseAsync();
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
        await Assertions.Expect(page.Locator(".sidebar").First).ToContainTextAsync("GetEndpoint");

        await page.CloseAsync();
    }

    [Fact]
    public async Task SwaggerUISilentlyAuthenticatesAndCallsProtectedEndpoint()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiWithSwaggerUIAsync(page);

        var redirectUri = $"{Fixture.BaseUrl}/{Tenant.Prefix}/OrchardCore.OpenApi/openapi-oidc-silent.html";

        await ConfigureOpenIdServerForAuthorizationCodeAsync(page, $"/{Tenant.Prefix}");
        await RegisterOpenApiSilentClientAsync(page, $"/{Tenant.Prefix}", redirectUri);

        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");

        // Opening the Swagger page loads the injected openapi-ui-auth bundle, which silently
        // acquires a bearer token from the existing admin session (prompt=none). No Authorize
        // click; give the hidden-iframe handshake a moment to complete before the first call.
        await page.GotoAsync($"/{Tenant.Prefix}/swagger");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(3000);

        // Use the Contents module's GET content-item endpoint (enabled by the Blog recipe) as
        // the protected "Api"-scheme endpoint to prove the token is attached: an unauthenticated
        // request to it returns 401, an authenticated one 404 for an unknown id.
        var operation = page.Locator("#operations-GetEndpoint-ApiGetContentItem");
        await operation.Locator(".opblock-summary").ClickAsync();
        await operation.Locator("button.try-out__btn").ClickAsync();
        await operation.Locator("tr[data-param-name='contentItemId'] input").FillAsync("does-not-exist");

        var response = await page.RunAndWaitForResponseAsync(
            async () => await operation.Locator("button.execute").ClickAsync(),
            r => r.Url.Contains("/api/content/"));

        Assert.NotEqual(401, response.Status);
        Assert.NotEqual(403, response.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ScalarUISilentlyAuthenticatesAndCallsProtectedEndpoint()
    {
        var page = await Fixture.CreatePageAsync();
        await EnableOpenApiAsync(page);
        await FeatureHelper.EnableFeatureAsync(page, $"/{Tenant.Prefix}", "OrchardCore.OpenApi.ScalarUI");

        var redirectUri = $"{Fixture.BaseUrl}/{Tenant.Prefix}/OrchardCore.OpenApi/openapi-oidc-silent.html";

        await ConfigureOpenIdServerForAuthorizationCodeAsync(page, $"/{Tenant.Prefix}");
        await RegisterOpenApiSilentClientAsync(page, $"/{Tenant.Prefix}", redirectUri);

        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");

        // Opening the Scalar page loads the injected openapi-ui-auth bundle, which wraps fetch and
        // silently acquires a bearer token from the existing admin session (prompt=none). No
        // Authorize click; give the hidden-iframe handshake a moment before sending a request.
        await page.GotoAsync($"/{Tenant.Prefix}/scalar/v1");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(3000);

        // Use the Contents module's GET content-item endpoint (enabled by the Blog recipe) as
        // the protected "Api"-scheme endpoint to prove the token is attached: an unauthenticated
        // request to it returns 401. The unfilled {contentItemId} placeholder is sent as a
        // literal segment, which still matches the route and exercises authentication.
        await page.Locator(".sidebar a", new() { HasText = "GetEndpoint" }).First.ClickAsync();
        var operation = page.Locator("[id='tag/getendpoint/GET/api/content/{contentItemId}']");
        await operation.Locator("button.show-api-client-button").ClickAsync();

        var response = await page.RunAndWaitForResponseAsync(
            async () => await page.GetByRole(AriaRole.Button, new() { Name = "Send Request", Exact = true }).ClickAsync(),
            r => r.Url.Contains("/api/content/"));

        Assert.NotEqual(401, response.Status);
        Assert.NotEqual(403, response.Status);

        await page.CloseAsync();
    }
}
