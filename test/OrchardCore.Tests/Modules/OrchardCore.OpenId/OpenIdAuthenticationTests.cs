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

            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.Users"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Server"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Validation"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId"));

            var httpClient = context.Client;

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            var applications = await session.Query<OpenIdApplication, OpenIdApplicationIndex>(OpenIdApplication.OpenIdCollection).ListAsync();

            Assert.Single(applications);

            var application = applications.First();
            Assert.True(application.ClientId == clientId);
            Assert.Contains(redirectUri, application.RedirectUris);
            Assert.Equal("implicit", application.ConsentType);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Token, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code, application.Permissions);

            Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, application.Requirements);

            // Visit the login page to get the AntiForgery token.
            var loginGetRequest = await httpClient.GetAsync("Login", CancellationToken.None);

            var loginFormData = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(loginGetRequest) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();

            var loginPostRequest = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}?loggedIn=true", loginFormData, loginGetRequest);

            // Login
            var loginPostResponse = await httpClient.SendAsync(loginPostRequest, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);

            var loginRequestRedirectToLocation = loginPostResponse.Headers.Location?.ToString();

            Assert.NotEmpty(loginRequestRedirectToLocation);
            Assert.Contains("loggedIn=true", loginRequestRedirectToLocation);

            var cookies = CookiesHelper.ExtractCookies(loginPostResponse);

            Assert.Contains("orchauth_" + shellSettings.Name, cookies.Keys);

            var codeVerifier = GenerateCodeVerifier();
            var challengeCode = GenerateCodeChallenge(codeVerifier);
            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "scope", "openid offline_access" },
                { "code_challenge_method", "S256" },
                { "code_verifier", codeVerifier },
                { "code_challenge", challengeCode },
            };

            var authorizeRequestMessage = HttpRequestHelper.CreatePostMessage("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(authorizeRequestMessage, cookies);

            Assert.True(authorizeRequestMessage.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authorizeResponse = await httpClient.SendAsync(authorizeRequestMessage, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

            var authorizeRequestRedirectToLocation = authorizeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(authorizeRequestRedirectToLocation);

            var authorizationCodeRequestMessage = HttpRequestHelper.CreateGetMessage(authorizeRequestRedirectToLocation);
            CookiesHelper.AddCookiesToRequest(authorizationCodeRequestMessage, cookies);

            var authorizationCodeResponse = await httpClient.SendAsync(authorizationCodeRequestMessage);

            Assert.Equal(HttpStatusCode.Redirect, authorizationCodeResponse.StatusCode);

            var finalRedirect = authorizationCodeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect);

            // Extract the authorization code from the query string.
            var queryParameters = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = queryParameters["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            // One one task should succeed since OpenId will only allow one access_token exchange for every authorization_code.
            var taskOne = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree);

            Assert.Single(tokens);
        });
    }

    [Fact]
    public async Task OpenId_CodeFlow_CanLogout()
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

            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.Users"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Server"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Validation"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId"));

            var httpClient = context.Client;

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            var applications = await session.Query<OpenIdApplication, OpenIdApplicationIndex>(OpenIdApplication.OpenIdCollection).ListAsync();

            Assert.Single(applications);

            var application = applications.First();
            Assert.True(application.ClientId == clientId);
            Assert.Contains(redirectUri, application.RedirectUris);
            Assert.Equal("implicit", application.ConsentType);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Token, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code, application.Permissions);

            Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, application.Requirements);

            // Visit the login page to get the AntiForgery token.
            var loginGetRequest = await httpClient.GetAsync("Login", CancellationToken.None);

            var loginFormData = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(loginGetRequest) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();

            var loginPostRequest = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}?loggedIn=true", loginFormData, loginGetRequest);

            // Login
            var loginPostResponse = await httpClient.SendAsync(loginPostRequest, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);

            var loginRequestRedirectToLocation = loginPostResponse.Headers.Location?.ToString();

            Assert.NotEmpty(loginRequestRedirectToLocation);
            Assert.Contains("loggedIn=true", loginRequestRedirectToLocation);

            var cookies = CookiesHelper.ExtractCookies(loginPostResponse);

            Assert.Contains("orchauth_" + shellSettings.Name, cookies.Keys);

            var codeVerifier = GenerateCodeVerifier();
            var challengeCode = GenerateCodeChallenge(codeVerifier);
            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "scope", "openid offline_access" },
                { "code_challenge_method", "S256" },
                { "code_verifier", codeVerifier },
                { "code_challenge", challengeCode },
            };

            var authorizeRequestMessage = HttpRequestHelper.CreatePostMessage("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(authorizeRequestMessage, cookies);

            Assert.True(authorizeRequestMessage.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authorizeResponse = await httpClient.SendAsync(authorizeRequestMessage, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

            var authorizeRequestRedirectToLocation = authorizeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(authorizeRequestRedirectToLocation);

            var authorizationCodeRequestMessage = HttpRequestHelper.CreateGetMessage(authorizeRequestRedirectToLocation);
            CookiesHelper.AddCookiesToRequest(authorizationCodeRequestMessage, cookies);

            var authorizationCodeResponse = await httpClient.SendAsync(authorizationCodeRequestMessage);

            Assert.Equal(HttpStatusCode.Redirect, authorizationCodeResponse.StatusCode);

            var finalRedirect = authorizationCodeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect);

            // Extract the authorization code from the query string.
            var queryParameters = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = queryParameters["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            // One one task should succeed since OpenId will only allow one access_token exchange for every authorization_code.
            await ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            Assert.Single(tokens);

            // https://github.com/OrchardCMS/OrchardCore/issues/17472

            var logoutGetRequestMessage = HttpRequestHelper.CreatePostMessage("connect/logout", []);
            // CookiesHelper.AddCookiesToRequest(logoutGetRequestMessage, cookies);
            logoutGetRequestMessage.Content = new FormUrlEncodedContent([]);
            logoutGetRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.First());

            var logoutResponse = await httpClient.SendAsync(logoutGetRequestMessage, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        });
    }

    private static async Task ExchangeCodeForTokenAsync(HttpClient httpClient, IDictionary<string, string> cookies, string authorizationCode, string clientId, string redirectUri, string codeVerifier, ConcurrentBag<string> tokens)
    {
        var data = new Dictionary<string, string>()
        {
            { "client_id", clientId },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier },
        };

        var request = HttpRequestHelper.CreatePostMessage("connect/token", data);
        CookiesHelper.AddCookiesToRequest(request, cookies);

        var tokenResponse = await httpClient.SendAsync(request, CancellationToken.None);

        if (tokenResponse.IsSuccessStatusCode)
        {
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();

            var accessToken = tokenResult["access_token"]?.ToString();

            Assert.NotEmpty(accessToken);

            tokens.Add(accessToken);
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
