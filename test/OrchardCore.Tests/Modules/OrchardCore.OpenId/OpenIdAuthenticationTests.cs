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

            var application = applications[0];
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
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var requestData = new Dictionary<string, string>
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
            var taskOne = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree);

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

            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.Users"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Server"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId.Validation"));
            Assert.True(await featureManager.IsFeatureEnabledAsync("OrchardCore.OpenId"));

            var httpClient = context.Client;

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            var applications = await session.Query<OpenIdApplication, OpenIdApplicationIndex>(OpenIdApplication.OpenIdCollection).ListAsync();

            Assert.Single(applications);

            var application = applications[0];
            Assert.True(application.ClientId == clientId);
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
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "request_uri", await GetRequestUriAsync(httpClient, clientId, redirectUri, codeChallenge) },
            };

            var authorizeRequestMessage = HttpRequestHelper.CreatePostMessage("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(authorizeRequestMessage, cookies);

            Assert.True(authorizeRequestMessage.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authorizeResponse = await httpClient.SendAsync(authorizeRequestMessage, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

            var finalRedirect = authorizeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect);

            // Extract the authorization code from the query string.
            var queryParameters = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = queryParameters["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            // One one task should succeed since OpenId will only allow one access_token exchange for every authorization_code.
            var taskOne = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree);

            Assert.Single(tokens);
        });
    }

    [Fact]
    public async Task OpenIdPasswordGrant_RateLimitExceededReturnsTooManyRequests_Succeeds()
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        const string clientId = "password-flow-client";

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    { "name", "Feature" },
                    { "enable", new JsonArray(
                        "OrchardCore.Users",
                        "OrchardCore.RateLimits",
                        "OrchardCore.OpenId.Server",
                        "OrchardCore.OpenId.Validation",
                        "OrchardCore.OpenId") },
                },
                new JsonObject
                {
                    { "name", "OpenIdServerSettings" },
                    { "EnableTokenEndpoint", true },
                    { "AllowPasswordFlow", true },
                },
                new JsonObject
                {
                    { "name", "OpenIdApplication" },
                    { "ClientId", clientId },
                    { "DisplayName", "Password Flow Test Application" },
                    { "Type", "public" },
                    { "AllowPasswordFlow", true },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        HttpResponseMessage response = null;

        for (var i = 0; i < 11; i++)
        {
            var request = HttpRequestHelper.CreatePostMessage("connect/token", new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "password" },
                { "username", "admin" },
                { "password", "WrongPassword01_" },
            });

            response = await context.Client.SendAsync(request, CancellationToken.None);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task OpenId_Logout_WithValidIdTokenHint_SignsOutWithoutConfirmation_WhenConfirmationIsDisabled()
    {
        var context = await CreateLogoutSiteContextAsync(requireEndSessionConfirmation: false);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var httpClient = context.Client;
            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var session = await SignInAndRequestTokensAsync(httpClient, shellSettings, LogoutClientId);
            var postLogoutRedirectUri = GetPostLogoutRedirectUri(httpClient, LogoutClientId);

            var response = await SendLogoutRequestAsync(httpClient, session.Cookies, new Dictionary<string, string>
            {
                { OpenIddictConstants.Parameters.IdTokenHint, session.IdToken },
                { OpenIddictConstants.Parameters.ClientId, LogoutClientId },
                { OpenIddictConstants.Parameters.PostLogoutRedirectUri, postLogoutRedirectUri },
            });

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith(postLogoutRedirectUri, response.Headers.Location?.ToString());

            // The local authentication cookie must be removed as well.
            var cookies = CookiesHelper.ExtractCookies(response);
            Assert.True(cookies.TryGetValue(session.AuthenticationCookieName, out var cookieValue));
            Assert.Empty(cookieValue);
        });
    }

    [Fact]
    public async Task OpenId_Logout_WithValidIdTokenHint_ShowsConfirmation_WhenConfirmationIsRequiredByDefault()
    {
        var context = await CreateLogoutSiteContextAsync(requireEndSessionConfirmation: null);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var httpClient = context.Client;
            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var session = await SignInAndRequestTokensAsync(httpClient, shellSettings, LogoutClientId);

            var response = await SendLogoutRequestAsync(httpClient, session.Cookies, new Dictionary<string, string>
            {
                { OpenIddictConstants.Parameters.IdTokenHint, session.IdToken },
                { OpenIddictConstants.Parameters.ClientId, LogoutClientId },
                { OpenIddictConstants.Parameters.PostLogoutRedirectUri, GetPostLogoutRedirectUri(httpClient, LogoutClientId) },
            });

            await AssertConfirmationPromptAsync(response, session.AuthenticationCookieName);
        });
    }

    [Fact]
    public async Task OpenId_Logout_WithIdTokenHintIssuedToAnotherClient_DoesNotSignOut()
    {
        var context = await CreateLogoutSiteContextAsync(requireEndSessionConfirmation: false);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var httpClient = context.Client;
            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var session = await SignInAndRequestTokensAsync(httpClient, shellSettings, LogoutClientId);
            var otherPostLogoutRedirectUri = GetPostLogoutRedirectUri(httpClient, OtherLogoutClientId);

            var response = await SendLogoutRequestAsync(httpClient, session.Cookies, new Dictionary<string, string>
            {
                { OpenIddictConstants.Parameters.IdTokenHint, session.IdToken },
                { OpenIddictConstants.Parameters.ClientId, OtherLogoutClientId },
                { OpenIddictConstants.Parameters.PostLogoutRedirectUri, otherPostLogoutRedirectUri },
            });

            Assert.False(response.Headers.Location?.ToString().StartsWith(otherPostLogoutRedirectUri, StringComparison.Ordinal) ?? false);
            Assert.DoesNotContain(session.AuthenticationCookieName, CookiesHelper.ExtractCookies(response).Keys);
        });
    }

    [Theory]
    [InlineData("invalid-token")]
    [InlineData("access-token")]
    [InlineData("no-client")]
    public async Task OpenId_Logout_WithUnverifiableIdTokenHint_ShowsConfirmation_WhenConfirmationIsDisabled(string scenario)
    {
        var context = await CreateLogoutSiteContextAsync(requireEndSessionConfirmation: false);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var httpClient = context.Client;
            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var session = await SignInAndRequestTokensAsync(httpClient, shellSettings, LogoutClientId);
            var postLogoutRedirectUri = GetPostLogoutRedirectUri(httpClient, LogoutClientId);

            var parameters = scenario switch
            {
                "invalid-token" => new Dictionary<string, string>
                {
                    { OpenIddictConstants.Parameters.IdTokenHint, "invalid-token" },
                    { OpenIddictConstants.Parameters.ClientId, LogoutClientId },
                    { OpenIddictConstants.Parameters.PostLogoutRedirectUri, postLogoutRedirectUri },
                },
                "access-token" => new Dictionary<string, string>
                {
                    { OpenIddictConstants.Parameters.IdTokenHint, session.AccessToken },
                    { OpenIddictConstants.Parameters.ClientId, LogoutClientId },
                    { OpenIddictConstants.Parameters.PostLogoutRedirectUri, postLogoutRedirectUri },
                },

                // A valid hint that can't be bound to a client application.
                "no-client" => new Dictionary<string, string>
                {
                    { OpenIddictConstants.Parameters.IdTokenHint, session.IdToken },
                },
                _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
            };

            var response = await SendLogoutRequestAsync(httpClient, session.Cookies, parameters);

            await AssertConfirmationPromptAsync(response, session.AuthenticationCookieName);
        });
    }

    private const string LogoutClientId = "logout_client";
    private const string OtherLogoutClientId = "other_logout_client";

    private sealed record LogoutTestSession(
        IDictionary<string, string> Cookies,
        string AuthenticationCookieName,
        string IdToken,
        string AccessToken);

    private static async Task<SiteContext> CreateLogoutSiteContextAsync(bool? requireEndSessionConfirmation)
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var serverSettingsStep = new JsonObject
        {
            { "name", "OpenIdServerSettings" },
            { "EnableAuthorizationEndpoint", true },
            { "EnableTokenEndpoint", true },
            { "EnableLogoutEndpoint", true },
            { "AllowAuthorizationCodeFlow", true },
        };

        if (requireEndSessionConfirmation is not null)
        {
            serverSettingsStep.Add("RequireEndSessionConfirmation", requireEndSessionConfirmation.Value);
        }

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    { "name", "Feature" },
                    { "enable", new JsonArray(
                        "OrchardCore.Users",
                        "OrchardCore.OpenId.Server",
                        "OrchardCore.OpenId.Validation",
                        "OrchardCore.OpenId") },
                },
                serverSettingsStep,
                CreateLogoutApplicationStep(context.Client, LogoutClientId),
                CreateLogoutApplicationStep(context.Client, OtherLogoutClientId),
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }

    private static JsonObject CreateLogoutApplicationStep(HttpClient httpClient, string clientId)
        => new()
        {
            { "name", "OpenIdApplication" },
            { "ClientId", clientId },
            { "DisplayName", clientId },
            { "Type", "public" },
            { "ConsentType", "implicit" },
            { "AllowAuthorizationCodeFlow", true },
            { "RequireProofKeyForCodeExchange", true },
            { "AllowLogoutEndpoint", true },
            { "RedirectUris", GetRedirectUri(httpClient, clientId) },
            { "PostLogoutRedirectUris", GetPostLogoutRedirectUri(httpClient, clientId) },
        };

    private static string GetRedirectUri(HttpClient httpClient, string clientId)
        => httpClient.BaseAddress + "signin-oidc-" + clientId;

    private static string GetPostLogoutRedirectUri(HttpClient httpClient, string clientId)
        => httpClient.BaseAddress + "signout-callback-oidc-" + clientId;

    private static async Task<LogoutTestSession> SignInAndRequestTokensAsync(HttpClient httpClient, ShellSettings shellSettings, string clientId)
    {
        // Visit the login page to get the AntiForgery token.
        var loginGetRequest = await httpClient.GetAsync("Login", CancellationToken.None);

        var loginFormData = new Dictionary<string, string>
        {
            {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(loginGetRequest) },
            {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
            {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
        };

        var loginPostRequest = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}", loginFormData, loginGetRequest);
        var loginPostResponse = await httpClient.SendAsync(loginPostRequest, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);

        var cookies = CookiesHelper.ExtractCookies(loginPostResponse);
        var authenticationCookieName = "orchauth_" + shellSettings.Name;

        Assert.Contains(authenticationCookieName, cookies.Keys);

        var redirectUri = GetRedirectUri(httpClient, clientId);
        var codeVerifier = GenerateCodeVerifier();

        var authorizeRequest = HttpRequestHelper.CreatePostMessage("connect/authorize", new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "scope", "openid" },
            { "code_challenge_method", "S256" },
            { "code_challenge", GenerateCodeChallenge(codeVerifier) },
        });
        CookiesHelper.AddCookiesToRequest(authorizeRequest, cookies);

        var authorizeResponse = await httpClient.SendAsync(authorizeRequest, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Redirect, authorizeResponse.StatusCode);

        // Authorization requests are cached by OpenIddict, which redirects the user agent to the same endpoint.
        var cachedAuthorizeRequest = HttpRequestHelper.CreateGetMessage(authorizeResponse.Headers.Location?.ToString());
        CookiesHelper.AddCookiesToRequest(cachedAuthorizeRequest, cookies);

        var authorizationCodeResponse = await httpClient.SendAsync(cachedAuthorizeRequest, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Redirect, authorizationCodeResponse.StatusCode);

        var callbackUri = authorizationCodeResponse.Headers.Location?.ToString();

        Assert.StartsWith(redirectUri, callbackUri);

        var authorizationCode = HttpUtility.ParseQueryString(new Uri(callbackUri).Query)["code"];

        Assert.NotEmpty(authorizationCode);

        var tokenRequest = HttpRequestHelper.CreatePostMessage("connect/token", new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier },
        });

        var tokenResponse = await httpClient.SendAsync(tokenRequest, CancellationToken.None);

        Assert.True(tokenResponse.IsSuccessStatusCode, await tokenResponse.Content.ReadAsStringAsync());

        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
        var idToken = tokenResult[OpenIddictConstants.Parameters.IdToken]?.ToString();
        var accessToken = tokenResult[OpenIddictConstants.Parameters.AccessToken]?.ToString();

        Assert.NotEmpty(idToken);
        Assert.NotEmpty(accessToken);

        return new LogoutTestSession(cookies, authenticationCookieName, idToken, accessToken);
    }

    private static async Task<HttpResponseMessage> SendLogoutRequestAsync(HttpClient httpClient, IDictionary<string, string> cookies, Dictionary<string, string> parameters)
    {
        var query = string.Join('&', parameters.Select(parameter => $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        var logoutRequest = HttpRequestHelper.CreateGetMessage("connect/logout?" + query);
        CookiesHelper.AddCookiesToRequest(logoutRequest, cookies);

        var logoutResponse = await httpClient.SendAsync(logoutRequest, CancellationToken.None);

        // End session requests are cached by OpenIddict, which redirects the user agent to the same endpoint
        // with a request_uri parameter. The logout action is only invoked when that second request is processed.
        var location = logoutResponse.Headers.Location?.ToString();
        if (logoutResponse.StatusCode != HttpStatusCode.Redirect || location?.Contains("request_uri=", StringComparison.Ordinal) != true)
        {
            return logoutResponse;
        }

        var cachedLogoutRequest = HttpRequestHelper.CreateGetMessage(location);
        CookiesHelper.AddCookiesToRequest(cachedLogoutRequest, cookies);

        return await httpClient.SendAsync(cachedLogoutRequest, CancellationToken.None);
    }

    private static async Task AssertConfirmationPromptAsync(HttpResponseMessage response, string authenticationCookieName)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("submit.Accept", await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain(authenticationCookieName, CookiesHelper.ExtractCookies(response).Keys);
    }

    private static async Task ExchangeCodeForTokenAsync(HttpClient httpClient, string authorizationCode, string clientId, string redirectUri, string codeVerifier, ConcurrentBag<string> tokens)
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

        var tokenResponse = await httpClient.SendAsync(request, CancellationToken.None);

        if (tokenResponse.IsSuccessStatusCode)
        {
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();

            var accessToken = tokenResult[OrchardCoreConstants.TokenNames.AccessToken]?.ToString();

            Assert.NotEmpty(accessToken);

            tokens.Add(accessToken);
        }
    }

    private static async Task<string> GetRequestUriAsync(HttpClient httpClient, string clientId, string redirectUri, string codeChallenge)
    {
        var data = new Dictionary<string, string>()
        {
            { "client_id", clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "scope", "openid offline_access" },
            { "code_challenge_method", "S256" },
            { "code_challenge", codeChallenge },
        };

        var request = HttpRequestHelper.CreatePostMessage("connect/par", data);

        var response = await httpClient.SendAsync(request, CancellationToken.None);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonObject>();
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
