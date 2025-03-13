using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using OrchardCore.Deployment.Services;
using OrchardCore.Environment.Shell;
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

        var clientId = "id";

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                {"name", "OpenIdApplication"},
                {"ClientId", clientId},
                {"DisplayName", "Test Application"},
                {"Type", "public"},
                {"ConsentType", "Implicit"},
                {"AllowAuthorizationCodeFlow", true},
                {"RequireProofKeyForCodeExchange", true},
                {"AllowRefreshTokenFlow", true},
                {"RedirectUris", redirectUri},
                {"RoleEntries", new JsonArray
                    {
                        new JsonObject
                        {
                            {"name", "role1"},
                            {"selected", true},
                        },
                        new JsonObject
                        {
                            {"name", "role2"},
                        },
                        new JsonObject
                        {
                            {"name", "scope1"},
                            {"selected", true},
                        },
                    }
                },
            },
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

            var url = $"connect/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=openid%20offline_access&code_challenge_method=S256&code_challenge={Uri.EscapeDataString(challengeCode)}";

            var requestForAuthorize = HttpRequestHelper.CreateGetMessageWithCookies(url, postLoginResponse);

            Assert.True(requestForAuthorize.Headers.Contains("Cookie"), "Cookie header is missing from request.");

            var authResponse = await httpClient.SendAsync(requestForAuthorize, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, authResponse.StatusCode);

            var finalRedirect = authResponse.Headers.Location?.ToString();

            if (finalRedirect != null && finalRedirect.StartsWith(redirectUri))
            {
                // Extract the authorization code from the query string.
                var codeParams = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
                var authorizationCode = codeParams["code"];

                Assert.NotNull(authorizationCode);

                // The test should be to exchange the code for an access token multiple times at the same time.
                // Exchange the authorization code for an access token.
                await ExchangeCodeForTokenAsync(httpClient, postLoginResponse, authorizationCode, clientId, redirectUri, codeVerifier);
            }

        });
    }

    private static async Task<string> ExchangeCodeForTokenAsync(HttpClient httpClient, HttpResponseMessage response, string authorizationCode, string clientId, string redirectUri, string codeVerifier)
    {
        var data = new Dictionary<string, string>()
        {
            { "client_id", clientId },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier },
        };

        var request = HttpRequestHelper.CreatePostMessageWithCookies("connect/token", data, response);

        var tokenResponse = await httpClient.SendAsync(request, CancellationToken.None);

        tokenResponse.EnsureSuccessStatusCode();

        return await tokenResponse.Content.ReadAsStringAsync();
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
