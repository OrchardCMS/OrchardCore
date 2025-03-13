using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using OrchardCore.Deployment.Services;
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
                    "OrchardCore.OpenId",
                    "OrchardCore.OpenId.Client")
                },
            },
        };

        var recipe = new JsonObject
        {
            {"steps", recipeSteps},
        };

        var recipeString = JsonSerializer.Serialize(recipe);

        await RunRecipeAsync(context, recipe);

        var codeVerifier = GenerateCodeVerifier();

        var challengeCode = GenerateCodeChallenge(codeVerifier);

        var url = $"connect/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=openid offline_access&code_challenge_method=S256&code_challenge={challengeCode}";

        var authResponse = await context.Client.GetAsync(url, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Redirect, authResponse.StatusCode);

        var d = await authResponse.Content.ReadAsStringAsync(CancellationToken.None);

        var redirectLocation = authResponse.Headers.Location?.ToString();

        if (!string.IsNullOrEmpty(redirectLocation) && redirectLocation.Contains("request_id"))
        {
            var uri = new Uri(redirectLocation);
            var redirectParams = HttpUtility.ParseQueryString(uri.Query);
            var requestId = redirectParams["request_id"];

            var returnUrl = HttpUtility.UrlEncode($"/connect/authorize?request_id={requestId}&prompt=continue");

            // Visit the login page to get the AntiForgery token.
            var getLoginResponse = await context.Client.GetAsync($"Login?ReturnUrl={returnUrl}", CancellationToken.None);

            var loginForm = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(getLoginResponse) },
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}", "admin"},
                {$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}", "Password01_"},
            };

            var requestForLoginPost = PostRequestHelper.CreateMessageWithCookies($"Login?ReturnUrl={returnUrl}", loginForm, getLoginResponse);

            // Login
            var postLoginResponse = await context.Client.SendAsync(requestForLoginPost, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, postLoginResponse.StatusCode);

            // After login, follow the next redirect to get the authorization code
            var tt = await postLoginResponse.Content.ReadAsStringAsync(CancellationToken.None);

            var finalRedirect = postLoginResponse.Headers.Location?.ToString();

            if (finalRedirect != null && finalRedirect.StartsWith(redirectUri))
            {
                // Extract the authorization code from the query string.
                var codeParams = HttpUtility.ParseQueryString(new Uri(finalRedirect).Query);
                var authorizationCode = codeParams["code"];

                Assert.NotNull(authorizationCode);

                // The test should be to exchange the code for an access token multiple times at the same time.

                // Exchange the authorization code for an access token.
                await ExchangeCodeForTokenAsync(context, authorizationCode, clientId, redirectUri, codeVerifier);
            }
        }
    }

    private static async Task<string> ExchangeCodeForTokenAsync(SiteContext context, string authorizationCode, string clientId, string redirectUri, string codeVerifier)
    {
        var tokenResponse = await context.Client.PostAsync("connect/token", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("code_verifier", codeVerifier) // Required for PKCE
        }));

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
                    var bytes = Encoding.UTF8.GetBytes(data.ToJsonString());

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
