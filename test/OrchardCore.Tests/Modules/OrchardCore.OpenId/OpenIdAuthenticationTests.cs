using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Web;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.Users;
using OrchardCore.Tests.OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdAuthenticationTests
{
    [Fact]
    public async Task OpenId_CodeFlow_CanExchangeAuthorizationCodeForAccessTokenOnlyOnce()
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var redirectUri = context.Client.BaseAddress.ToString() + "signin-oidc";

        var clientId = "test_id";

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                {"name", "Feature"},
                {"enable", new JsonArray(
                    "OrchardCore.Users",
                    "OrchardCore.OpenId.Server",
                    "OrchardCore.OpenId.Validation",
                    "OrchardCore.OpenId")
                },
            },
            new JsonObject
            {
                {"name", "OpenIdApplication"},
                {"ClientId", clientId},
                {"DisplayName", "Test Application"},
                {"Type", "public"},
                {"ConsentType", "implicit"},
                {"AllowAuthorizationCodeFlow", true},
                {"RequireProofKeyForCodeExchange", true},
                {"AllowRefreshTokenFlow", true},
                {"RedirectUris", redirectUri},
            },
        };

        var recipe = new JsonObject
        {
            {"steps", recipeSteps},
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featureManager = scope.ServiceProvider.GetService<IShellFeaturesManager>();

            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.Users").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Server").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Validation").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId").ConfigureAwait(false));

            var httpClient = context.Client;

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            var applications = await session.Query<OpenIdApplication, OpenIdApplicationIndex>(OpenIdApplication.OpenIdCollection).ListAsync().ConfigureAwait(false);

            Assert.Single(applications);

            var application = applications.First();
            Assert.Equal(application.ClientId, clientId);
            Assert.Contains(redirectUri, application.RedirectUris);
            Assert.Equal("implicit", application.ConsentType);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Token, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code, application.Permissions);

            Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, application.Requirements);

            // Visit the login page to get the AntiForgery token.
            var loginGetRequest = await httpClient.GetAsync("Login", CancellationToken.None).ConfigureAwait(false);

            var loginFormData = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(loginGetRequest).ConfigureAwait(false) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();

            var loginPostRequest = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}?loggedIn=true", loginFormData, loginGetRequest);

            // Login
            var loginPostResponse = await httpClient.SendAsync(loginPostRequest, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);

            var loginRequestRedirectToLocation = loginPostResponse.Headers.Location?.ToString();

            Assert.NotEmpty(loginRequestRedirectToLocation);
            Assert.Contains("loggedIn=true", loginRequestRedirectToLocation, StringComparison.Ordinal);

            var cookies = CookiesHelper.ExtractCookies(loginPostResponse);

            Assert.Contains("orchauth_" + shellSettings.Name, cookies.Keys);

            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var requestData = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "client_id", clientId },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "scope", "openid offline_access" },
                { "code_challenge_method", "S256" },
                { "code_challenge", codeChallenge },
            };

            var authorizeRequestMessage = HttpRequestHelper.CreatePostMessage("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(authorizeRequestMessage, cookies);

            Assert.True(authorizeRequestMessage.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authorizeResponse = await httpClient.SendAsync(authorizeRequestMessage, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

            var authorizeRequestRedirectToLocation = authorizeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(authorizeRequestRedirectToLocation);

            var authorizationCodeRequestMessage = HttpRequestHelper.CreateGetMessage(authorizeRequestRedirectToLocation);
            CookiesHelper.AddCookiesToRequest(authorizationCodeRequestMessage, cookies);

            var authorizationCodeResponse = await httpClient.SendAsync(authorizationCodeRequestMessage).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Redirect, authorizationCodeResponse.StatusCode);

            var finalRedirect = authorizationCodeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect, StringComparison.Ordinal);

            // Extract the authorization code from the query string.
            var queryParameters = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = queryParameters["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            // One one task should succeed since OpenId will only allow one access_token exchange for every authorization_code.
            var taskOne = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree).ConfigureAwait(false);

            Assert.Single(tokens);
        });
    }

    [Fact]
    public async Task OpenId_CodeFlowWithPushedAuthorizationRequests_CanExchangeAuthorizationCodeForAccessTokenOnlyOnce()
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var redirectUri = context.Client.BaseAddress.ToString() + "signin-oidc";

        var clientId = "test_id";

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                {"name", "Feature"},
                {"enable", new JsonArray(
                    "OrchardCore.Users",
                    "OrchardCore.OpenId.Server",
                    "OrchardCore.OpenId.Validation",
                    "OrchardCore.OpenId")
                },
            },
            new JsonObject
            {
                {"name", "OpenIdApplication"},
                {"ClientId", clientId},
                {"DisplayName", "Test Application"},
                {"Type", "public"},
                {"ConsentType", "implicit"},
                {"AllowAuthorizationCodeFlow", true},
                {"RequireProofKeyForCodeExchange", true},
                {"RequirePushedAuthorizationRequests", true},
                {"AllowRefreshTokenFlow", true},
                {"RedirectUris", redirectUri},
            },
        };

        var recipe = new JsonObject
        {
            {"steps", recipeSteps},
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featureManager = scope.ServiceProvider.GetService<IShellFeaturesManager>();

            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.Users").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Server").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Validation").ConfigureAwait(false));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId").ConfigureAwait(false));

            var httpClient = context.Client;

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            var applications = await session.Query<OpenIdApplication, OpenIdApplicationIndex>(OpenIdApplication.OpenIdCollection).ListAsync().ConfigureAwait(false);

            Assert.Single(applications);

            var application = applications.First();
            Assert.Equal(application.ClientId, clientId);
            Assert.Contains(redirectUri, application.RedirectUris);
            Assert.Equal("implicit", application.ConsentType);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.PushedAuthorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Token, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code, application.Permissions);

            Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, application.Requirements);
            Assert.Contains(OpenIddictConstants.Requirements.Features.PushedAuthorizationRequests, application.Requirements);

            // Visit the login page to get the AntiForgery token.
            var loginGetRequest = await httpClient.GetAsync("Login", CancellationToken.None).ConfigureAwait(false);

            var loginFormData = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(loginGetRequest).ConfigureAwait(false) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();

            var loginPostRequest = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}?loggedIn=true", loginFormData, loginGetRequest);

            // Login
            var loginPostResponse = await httpClient.SendAsync(loginPostRequest, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);

            var loginRequestRedirectToLocation = loginPostResponse.Headers.Location?.ToString();

            Assert.NotEmpty(loginRequestRedirectToLocation);
            Assert.Contains("loggedIn=true", loginRequestRedirectToLocation, StringComparison.Ordinal);

            var cookies = CookiesHelper.ExtractCookies(loginPostResponse);

            Assert.Contains("orchauth_" + shellSettings.Name, cookies.Keys);

            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            var requestData = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "client_id", clientId },
                { "request_uri", await GetRequestUriAsync(httpClient, clientId, redirectUri, codeChallenge).ConfigureAwait(false) },
            };

            var authorizeRequestMessage = HttpRequestHelper.CreatePostMessage("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(authorizeRequestMessage, cookies);

            Assert.True(authorizeRequestMessage.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authorizeResponse = await httpClient.SendAsync(authorizeRequestMessage, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

            var finalRedirect = authorizeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect, StringComparison.Ordinal);

            // Extract the authorization code from the query string.
            var queryParameters = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = queryParameters["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            // One one task should succeed since OpenId will only allow one access_token exchange for every authorization_code.
            var taskOne = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree).ConfigureAwait(false);

            Assert.Single(tokens);
        });
    }

    private static async Task ExchangeCodeForTokenAsync(HttpClient httpClient, string authorizationCode, string clientId, string redirectUri, string codeVerifier, ConcurrentBag<string> tokens)
    {
        var data = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "client_id", clientId },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier },
        };

        var request = HttpRequestHelper.CreatePostMessage("connect/token", data);

        var tokenResponse = await httpClient.SendAsync(request, CancellationToken.None).ConfigureAwait(false);

        if (tokenResponse.IsSuccessStatusCode)
        {
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>().ConfigureAwait(false);

            var accessToken = tokenResult["access_token"]?.ToString();

            Assert.NotEmpty(accessToken);

            tokens.Add(accessToken);
        }
    }

    private static async Task<string> GetRequestUriAsync(HttpClient httpClient, string clientId, string redirectUri, string codeChallenge)
    {
        var data = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "client_id", clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "scope", "openid offline_access" },
            { "code_challenge_method", "S256" },
            { "code_challenge", codeChallenge },
        };

        var request = HttpRequestHelper.CreatePostMessage("connect/par", data);

        var response = await httpClient.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonObject>().ConfigureAwait(false);
            var value = result["request_uri"]?.ToString();

            Assert.NotEmpty(value);

            return value;
        }
        else
        {
            throw new InvalidOperationException("An error response was returned by the pushed authorization endpoint.");
        }
    }

    private static string GenerateCodeVerifier()
    {
        var randomBytes = new byte[32];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));

        return Convert.ToBase64String(hashBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
