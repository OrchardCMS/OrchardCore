using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class McpClientRegistrationApiTests
{
    [Fact]
    public async Task TenantMcp_DiscoversRegistersAndCompletesValidatedOAuthFlow()
    {
        using var site = new SiteContext { RecipeName = "Blank" };
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.EnableFeaturesAsync(available.Where(feature => feature.Id == "OrchardCore.RemoteManagement.Mcp"), force: true);
        });
        await site.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        ShellSettings settings = null;
        await site.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IRemoteManagementTenantConfigurationService>().ConfigureAsync();
            settings = scope.ShellContext.Settings;
        });
        await SiteContext.ShellHost.ReleaseShellContextAsync(settings);

        using var client = SiteContext.Site.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = site.Client.BaseAddress,
            AllowAutoRedirect = false,
        });
        var cancellationToken = TestContext.Current.CancellationToken;
        using var challenge = await client.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", id = 1, method = "tools/list" }, cancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, challenge.StatusCode);
        Assert.Null(challenge.Headers.Location);
        Assert.Contains("orchardcore.management", challenge.Headers.WwwAuthenticate.ToString());

        using var protectedResource = await client.GetFromJsonAsync<JsonDocument>(".well-known/oauth-protected-resource/mcp", cancellationToken);
        var resource = protectedResource.RootElement.GetProperty("resource").GetString();
        Assert.Equal(new Uri(client.BaseAddress, "mcp").AbsoluteUri, resource);
        using var discovery = await client.GetFromJsonAsync<JsonDocument>(".well-known/openid-configuration", cancellationToken);
        var registrationEndpoint = discovery.RootElement.GetProperty("registration_endpoint").GetString();
        Assert.Equal(new Uri(client.BaseAddress, "connect/mcp/register").AbsoluteUri, registrationEndpoint);

        using var registration = await client.PostAsJsonAsync(registrationEndpoint, new
        {
            client_name = "Integration MCP client",
            redirect_uris = new[] { "https://client.example/callback" },
            grant_types = new[] { "authorization_code", "refresh_token" },
            response_types = new[] { "code" },
            token_endpoint_auth_method = "none",
            scope = "openid orchardcore.management offline_access",
            // Unknown privilege-bearing metadata must never be applied to the application.
            roles = new[] { "Administrator" },
            client_id = "orchardcore-cli",
            client_secret = "ignored-test-value",
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var registered = JsonDocument.Parse(await registration.Content.ReadAsStringAsync(cancellationToken));
        var clientId = registered.RootElement.GetProperty("client_id").GetString();
        Assert.StartsWith("orchardcore-mcp-", clientId);
        Assert.False(registered.RootElement.TryGetProperty("client_secret", out _));
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var application = await manager.FindByClientIdAsync(clientId, cancellationToken);
            Assert.NotNull(application);
            Assert.Empty(await manager.GetRolesAsync(application, cancellationToken));
            Assert.Equal(OpenIddictConstants.ClientTypes.Public, await manager.GetClientTypeAsync(application, cancellationToken));
        });

        var authorize = discovery.RootElement.GetProperty("authorization_endpoint").GetString();
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["response_type"] = "code",
            ["redirect_uri"] = "https://client.example/callback",
            ["scope"] = "openid orchardcore.management offline_access",
            ["code_challenge"] = WebEncoders.Base64UrlEncode(SHA256.HashData("test-verifier-with-enough-entropy-and-length-123456"u8)),
            ["code_challenge_method"] = "S256",
            ["resource"] = resource,
            ["state"] = "test-state",
        };
        using var authorization = await client.GetAsync(QueryHelpers.AddQueryString(authorize, parameters), cancellationToken);
        Assert.True(authorization.StatusCode == HttpStatusCode.Redirect, await authorization.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(client.BaseAddress.Host, authorization.Headers.Location.Host);

        parameters["redirect_uri"] = "https://attacker.example/callback";
        using var invalidCallback = await client.GetAsync(QueryHelpers.AddQueryString(authorize, parameters), cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalidCallback.StatusCode);
        Assert.Null(invalidCallback.Headers.Location);

        parameters["redirect_uri"] = "https://client.example/callback";
        parameters["resource"] = "https://another-tenant.example/mcp";
        using var wrongResource = await client.GetAsync(QueryHelpers.AddQueryString(authorize, parameters), cancellationToken);
        var error = wrongResource.Headers.Location?.ToString() ?? await wrongResource.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("invalid_target", error);

        parameters["resource"] = resource;
        parameters["code_challenge_method"] = "plain";
        using var weakPkce = await client.GetAsync(QueryHelpers.AddQueryString(authorize, parameters), cancellationToken);
        error = weakPkce.Headers.Location?.ToString() ?? await weakPkce.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("invalid_request", error);

        // Registration alone is not a bearer credential.
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientId);
        using var notAToken = await client.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", id = 2, method = "tools/list" }, cancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, notAToken.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var loginPage = await GetTenantPageAsync(client, authorization.Headers.Location, cancellationToken);
        var parser = new HtmlParser();
        using var loginDocument = await parser.ParseDocumentAsync(await loginPage.Content.ReadAsStringAsync(cancellationToken), cancellationToken);
        var loginForm = loginDocument.QuerySelector("form.auth-form");
        Assert.NotNull(loginForm);
        var loginFields = loginForm.QuerySelectorAll("input[type=hidden]")
            .ToDictionary(input => input.GetAttribute("name"), input => input.GetAttribute("value") ?? "");
        loginFields[loginForm.QuerySelector("input[name$=UserName]").GetAttribute("name")] = "admin";
        loginFields[loginForm.QuerySelector("input[type=password]").GetAttribute("name")] = "Password01_"; // The isolated SiteContext fixture's setup password.
        using var loggedIn = await client.PostAsync(loginForm.GetAttribute("action"), new FormUrlEncodedContent(loginFields), cancellationToken);
        Assert.True(loggedIn.StatusCode == HttpStatusCode.Redirect, await loggedIn.Content.ReadAsStringAsync(cancellationToken));

        using var configurationPage = await client.GetAsync("Admin/RemoteManagement/Mcp", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, configurationPage.StatusCode);
        var configurationHtml = await configurationPage.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("MCP authentication is ready", configurationHtml);
        Assert.Contains("Advanced: register a client manually", configurationHtml);

        parameters["code_challenge_method"] = "S256";
        using var consentPage = await GetTenantPageAsync(client, new Uri(QueryHelpers.AddQueryString(authorize, parameters)), cancellationToken);
        Assert.Equal(HttpStatusCode.OK, consentPage.StatusCode);
        using var consentDocument = await parser.ParseDocumentAsync(await consentPage.Content.ReadAsStringAsync(cancellationToken), cancellationToken);
        var consentForm = consentDocument.QuerySelector("button[name='submit.Accept']")?.Closest("form");
        Assert.NotNull(consentForm);
        var consentFields = consentForm.QuerySelectorAll("input[type=hidden]")
            .ToDictionary(input => input.GetAttribute("name"), input => input.GetAttribute("value") ?? "");
        Assert.True(consentFields.ContainsKey("__RequestVerificationToken"));
        consentFields["submit.Accept"] = "yes";
        using var consent = await client.PostAsync(consentForm.GetAttribute("action"), new FormUrlEncodedContent(consentFields), cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, consent.StatusCode);
        Assert.Equal("client.example", consent.Headers.Location.Host);
        var callback = QueryHelpers.ParseQuery(consent.Headers.Location.Query);
        Assert.Equal("test-state", callback["state"]);
        Assert.True(callback.ContainsKey("code"));

        var tokenEndpoint = discovery.RootElement.GetProperty("token_endpoint").GetString();
        var tokenFields = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = clientId,
            ["redirect_uri"] = parameters["redirect_uri"],
            ["code"] = callback["code"],
            ["code_verifier"] = "test-verifier-with-enough-entropy-and-length-123456",
            ["resource"] = resource,
        };
        tokenFields["resource"] = "https://another-tenant.example/mcp";
        using var invalidTokenResource = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenFields), cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalidTokenResource.StatusCode);
        Assert.Contains("invalid_target", await invalidTokenResource.Content.ReadAsStringAsync(cancellationToken));
        tokenFields["resource"] = resource;
        tokenFields["code_verifier"] = "incorrect-verifier-with-enough-length-123456789012";
        using var invalidVerifier = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenFields), cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalidVerifier.StatusCode);
        Assert.Contains("invalid_grant", await invalidVerifier.Content.ReadAsStringAsync(cancellationToken));
        tokenFields["code_verifier"] = "test-verifier-with-enough-entropy-and-length-123456";
        using var tokenResponse = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenFields), cancellationToken);
        Assert.True(tokenResponse.IsSuccessStatusCode, await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
        using var tokens = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
        var accessToken = tokens.RootElement.GetProperty("access_token").GetString();
        Assert.False(string.IsNullOrEmpty(accessToken));
        var refreshToken = tokens.RootElement.GetProperty("refresh_token").GetString();
        Assert.False(string.IsNullOrEmpty(refreshToken));

        // The real access token authorizes MCP, while a browser cookie alone does not.
        using var cookieOnly = await client.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", id = 3, method = "tools/list" }, cancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, cookieOnly.StatusCode);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
        using var tools = await client.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", id = 4, method = "tools/list" }, cancellationToken);
        Assert.True(tools.IsSuccessStatusCode, await tools.Content.ReadAsStringAsync(cancellationToken));
        Assert.Contains("tools", await tools.Content.ReadAsStringAsync(cancellationToken));
        client.DefaultRequestHeaders.Authorization = null;

        using var refreshed = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token", ["client_id"] = clientId,
            ["refresh_token"] = refreshToken, ["resource"] = resource,
        }), cancellationToken);
        Assert.True(refreshed.IsSuccessStatusCode, await refreshed.Content.ReadAsStringAsync(cancellationToken));

    }

    private static async Task<HttpResponseMessage> GetTenantPageAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
    {
        // OpenIddict may first redirect to an authorization request cached by request_uri.
        for (var i = 0; i < 5; i++)
        {
            uri = new Uri(client.BaseAddress, uri);
            Assert.True(client.BaseAddress.IsBaseOf(uri));
            var response = await client.GetAsync(uri, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Redirect)
            {
                return response;
            }

            uri = response.Headers.Location;
            response.Dispose();
        }

        throw new InvalidOperationException("Too many tenant redirects.");
    }

}
