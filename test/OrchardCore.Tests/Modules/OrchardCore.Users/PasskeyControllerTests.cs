using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OrchardCore.Users;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class PasskeyControllerTests
{
    private const string Origin = "http://localhost";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Password01_";

    [Fact]
    public async Task Create_WithValidAttestation_AddsPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);

        // Act
        var response = await RegisterPasskeyAsync(client, authenticator, "My laptop");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.EndsWith("/Security#passkeys", response.Headers.Location.ToString());

        var passkeys = await GetPasskeysAsync(context);
        var passkey = Assert.Single(passkeys);
        Assert.Equal("My laptop", passkey.Name);
        Assert.Equal(authenticator.CredentialId, passkey.CredentialId);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByPasskeyIdAsync(authenticator.CredentialId);

            Assert.Equal(AdminUserName, user?.UserName);
        });
    }

    [Fact]
    public async Task Create_WithAlreadyRegisteredCredential_DoesNotAddPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "First");

        // Act
        await RegisterPasskeyAsync(client, authenticator, "Second");

        // Assert
        var passkey = Assert.Single(await GetPasskeysAsync(context));
        Assert.Equal("First", passkey.Name);
    }

    [Fact]
    public async Task Login_WithRegisteredPasskey_SignsIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);

        var passkey = Assert.Single(await GetPasskeysAsync(context));
        Assert.NotNull(passkey.LastUsedUtc);
        Assert.Equal(1u, passkey.SignCount);
    }

    [Fact]
    public async Task Login_WithInvalidSignature_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator, tamper: true);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithoutRequestOptions_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        var client = CreateClient(context, out var cookies);
        var loginPage = await client.GetStringAsync("Login", TestContext.Current.CancellationToken);
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(loginPage);

        // A challenge the server never issued, as when replaying a credential captured elsewhere.
        var credential = authenticator.CreateAssertion(new JsonObject
        {
            ["challenge"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            ["rpId"] = "localhost",
        }.ToJsonString(), Origin);

        // Act
        var response = await PostAsync(client, "Passkeys/Login", token, new() { ["Credential"] = credential });

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Delete_WithPasskeyId_RemovesPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");

        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(page);

        // Act
        var response = await PostAsync(client, "OrchardCore.Users/Passkey/Delete", token, new()
        {
            ["Id"] = System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId),
        });

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(await GetPasskeysAsync(context));
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_WithPasskey_SignsIn()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(AdminUserName);

            await userManager.SetTwoFactorEnabledAsync(user, true);
        });

        var client = CreateClient(context, out var cookies);

        var loginResponse = await LoginWithPasswordAsync(client);
        Assert.Contains("LoginWithTwoFactorAuthentication", loginResponse.Headers.Location.ToString());

        var twoFactorPage = await client.GetStringAsync(loginResponse.Headers.Location, TestContext.Current.CancellationToken);
        Assert.Contains("data-passkey-two-factor", twoFactorPage);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(twoFactorPage);
        var options = await PostAsync(client, "OrchardCore.Users/Passkey/TwoFactorRequestOptions", token, []);
        options.EnsureSuccessStatusCode();

        var credential = authenticator.CreateAssertion(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin);

        // Act
        var response = await PostAsync(client, "LoginWithTwoFactorAuthentication", token, new()
        {
            ["VerificationCode"] = credential,
            ["CurrentProvider"] = UserConstants.PasskeyTwoFactorProvider,
            ["RememberMe"] = "false",
        });

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Security_WithTwoFactorAndPasskeys_ShowsBothSections()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);

        await LoginWithPasswordAsync(client);

        // Act
        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("id=\"two-factor\"", page);
        Assert.Contains("id=\"passkeys\"", page);
    }

    [Theory]
    [InlineData("TwoFactor", "/Security#two-factor")]
    [InlineData("Passkeys", "/Security#passkeys")]
    public async Task FormerPage_WhenRequested_RedirectsToSecuritySection(string path, string expectedLocation)
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);

        await LoginWithPasswordAsync(client);

        // Act
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}{expectedLocation}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Login_RateLimitExceeded_ReturnsTooManyRequests()
    {
        // Arrange
        var context = await GetSiteContextAsync("OrchardCore.RateLimits");

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 11; i++)
        {
            response = await context.Client.PostAsync("Passkeys/Login", new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> LoginWithPasswordAsync(HttpClient client)
    {
        var loginPage = await client.GetStringAsync("Login", TestContext.Current.CancellationToken);

        var response = await PostAsync(client, "Login", AntiForgeryHelper.ExtractAntiForgeryToken(loginPage), new()
        {
            [$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}"] = AdminUserName,
            [$"{nameof(LoginForm)}.{nameof(LoginViewModel.Password)}"] = AdminPassword,
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return response;
    }

    private static async Task<HttpResponseMessage> RegisterPasskeyAsync(HttpClient client, TestPasskeyAuthenticator authenticator, string name)
    {
        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(page);

        var options = await PostAsync(client, "OrchardCore.Users/Passkey/CreationOptions", token, []);
        options.EnsureSuccessStatusCode();

        var credential = authenticator.CreateAttestation(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin);

        return await PostAsync(client, "OrchardCore.Users/Passkey/Create", token, new()
        {
            ["Credential"] = credential,
            ["Name"] = name,
        });
    }

    private static async Task<HttpResponseMessage> LoginWithPasskeyAsync(HttpClient client, TestPasskeyAuthenticator authenticator, bool tamper = false)
    {
        var loginPage = await client.GetStringAsync("Login", TestContext.Current.CancellationToken);
        Assert.Contains("data-passkey-login", loginPage);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(loginPage);

        var options = await PostAsync(client, "OrchardCore.Users/Passkey/RequestOptions", token, []);
        options.EnsureSuccessStatusCode();

        var credential = authenticator.CreateAssertion(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin, tamper);

        return await PostAsync(client, "Passkeys/Login", token, new() { ["Credential"] = credential });
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string token, Dictionary<string, string> data)
    {
        data["__RequestVerificationToken"] = token;

        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(data),
        };

        // Browsers send the origin of the page with the credential, which the server checks.
        request.Headers.Add("Origin", Origin);

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<IList<UserPasskey>> GetPasskeysAsync(SiteContext context)
    {
        IList<UserPasskey> passkeys = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(AdminUserName) as User;

            passkeys = user.Passkeys;
        });

        return passkeys;
    }

    private static HttpClient CreateClient(SiteContext context, out CookieContainer cookies)
    {
        var handler = new CookieContainerHandler();
        cookies = handler.Container;

        return SiteContext.Site.CreateDefaultClient(context.Client.BaseAddress, handler);
    }

    private static async Task<SiteContext> GetSiteContextAsync(params string[] features)
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var enable = new JsonArray(UserConstants.Features.Passkeys);

        foreach (var feature in features)
        {
            enable.Add(feature);
        }

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    { "name", "feature" },
                    { "enable", enable },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }
}
