using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using OrchardCore.Deployment.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdAuthenticationTests
{
    [Fact]
    public async Task OpenIdCanExchangeCodeForAccessToken()
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var redirectUri = context.Client.BaseAddress.ToString() + "signin-oidc";

        var clientId = "test_public_client_id";

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

        await RunRecipeAsync(context, recipe);

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
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.Endpoints.Token, application.Permissions);
            Assert.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code, application.Permissions);

            Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, application.Requirements);

            // Visit the login page to get the AntiForgery token.
            var getLoginResponse = await httpClient.GetAsync("Login", CancellationToken.None);

            var loginForm = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(getLoginResponse) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();

            var requestForLoginPost = HttpRequestHelper.CreatePostMessageWithCookies($"Login?ReturnUrl=/{shellSettings.RequestUrlPrefix}?loggedIn=true", loginForm, getLoginResponse);

            // Login
            var postLoginResponse = await httpClient.SendAsync(requestForLoginPost, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, postLoginResponse.StatusCode);

            var redirectLocation = postLoginResponse.Headers.Location?.ToString();

            Assert.NotEmpty(redirectLocation);
            Assert.Contains("loggedIn=true", redirectLocation);

            var cookies = CookiesHelper.ExtractCookies(postLoginResponse);

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

            var requestForAuthorize = HttpRequestHelper.CreatePost("connect/authorize", requestData);
            CookiesHelper.AddCookiesToRequest(requestForAuthorize, cookies);

            Assert.True(requestForAuthorize.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authResponse = await httpClient.SendAsync(requestForAuthorize, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, authResponse.StatusCode);

            var authRedirectResponse = authResponse.Headers.Location?.ToString();

            var codeRequest = HttpRequestHelper.CreateGet(authRedirectResponse);
            CookiesHelper.AddCookiesToRequest(codeRequest, cookies);

            var codeResponse = await httpClient.SendAsync(codeRequest);

            Assert.Equal(HttpStatusCode.Redirect, codeResponse.StatusCode);

            var finalRedirect = codeResponse.Headers.Location?.ToString();

            Assert.NotEmpty(finalRedirect);
            Assert.StartsWith(redirectUri, finalRedirect);

            // Extract the authorization code from the query string.
            var codeParams = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
            var authorizationCode = codeParams["code"];

            Assert.NotEmpty(authorizationCode);

            var tokens = new ConcurrentBag<string>();

            var taskOne = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskTwo = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);
            var taskThree = ExchangeCodeForTokenAsync(httpClient, cookies, authorizationCode, clientId, redirectUri, codeVerifier, tokens);

            await Task.WhenAll(taskOne, taskTwo, taskThree);

            Assert.Equal(3, tokens.Count);
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

        var request = HttpRequestHelper.CreatePost("connect/token", data);
        CookiesHelper.AddCookiesToRequest(request, cookies);

        var tokenResponse = await httpClient.SendAsync(request, CancellationToken.None);

        tokenResponse.EnsureSuccessStatusCode();

        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();

        Assert.NotEmpty(tokenResult["access_token"]?.ToString());

        tokens.Add(tokenResult["access_token"]?.ToString());

        Debug.WriteLine("access token is " + tokenResult["access_token"]?.ToString());
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
            .Replace('/', '_'); // Base64 URL encoding
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));

        return Convert.ToBase64String(hashBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // Base64 URL encoding
    }

    private static async Task RunRecipeAsync(SiteContext context, JsonObject data)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var tempArchiveName = PathExtensions.GetTempFileName() + ".json";
            var tempArchiveFolder = PathExtensions.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempArchiveName, FileMode.Create))
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

                    await stream.WriteAsync(bytes);
                }

                Directory.CreateDirectory(tempArchiveFolder);
                File.Move(tempArchiveName, Path.Combine(tempArchiveFolder, "Recipe.json"));

                var deploymentManager = scope.ServiceProvider.GetRequiredService<IDeploymentManager>();

                await deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));
            }
            finally
            {
                if (File.Exists(tempArchiveName))
                {
                    File.Delete(tempArchiveName);
                }

                if (Directory.Exists(tempArchiveFolder))
                {
                    Directory.Delete(tempArchiveFolder, true);
                }
            }
        });
    }
}
