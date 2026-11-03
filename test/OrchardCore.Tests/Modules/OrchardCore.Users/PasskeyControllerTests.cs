using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OrchardCore.Users;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class PasskeyControllerTests
{
    private const string Origin = "http://localhost";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Password01_";
    private const string OtherUserName = "other";

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
    public async Task Login_WithoutUserVerification_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        authenticator.VerifiesUser = false;

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Theory]
    [InlineData("discouraged")]
    [InlineData("preferred")]
    // The passkey handler only enforces user verification for the exact value 'required'.
    [InlineData("Required")]
    public async Task Login_WithUnenforcedUserVerification_AsksForSecondFactor(string userVerificationRequirement)
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await SetUserVerificationRequirementAsync(context, userVerificationRequirement);
        await EnableTwoFactorAuthenticationAsync(context);

        authenticator.VerifiesUser = false;

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("LoginWithTwoFactorAuthentication", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithUnenforcedUserVerificationAndNoTwoFactorProvider_SignsIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await SetUserVerificationRequirementAsync(context, "discouraged");

        // Without the two-factor services, no provider can complete the second factor of a user who has it enabled.
        await EnableTwoFactorAuthenticationAsync(context);

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithRequiredTwoFactor_StillMeetsItAfterSignInIsRefreshed()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        string userId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            site.Put(new TwoFactorLoginSettings { RequireTwoFactorAuthentication = true });

            await siteService.UpdateSiteSettingsAsync(site);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            userId = await userManager.GetUserIdAsync(await userManager.FindByNameAsync(AdminUserName));
        });

        var client = CreateClient(context, out _);
        await LoginWithPasskeyAsync(client, authenticator);

        var adminResponse = await client.GetAsync("Admin", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var editPage = await client.GetStringAsync($"Admin/Users/Edit/{userId}", TestContext.Current.CancellationToken);

        // Act: saving their own user refreshes the sign-in of a user.
        var editResponse = await PostAsync(client, $"Admin/Users/Edit/{userId}", AntiForgeryHelper.ExtractAntiForgeryToken(editPage), new()
        {
            ["User.UserName"] = AdminUserName,
        });

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, editResponse.StatusCode);

        adminResponse = await client.GetAsync("Admin", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_LetsUserRequestConfirmationEmail()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await context.UsingTenantScopeAsync(async scope =>
        {
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<RegistrationOptions>>().CurrentValue.UsersMustValidateEmail = true;

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName) as User;

            user.EmailConfirmed = false;

            await userManager.UpdateAsync(user);
        });

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("ConfirmEmailSent", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchemail_" + context.TenantName]);
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

        await EnableTwoFactorAuthenticationAsync(context);

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
    public async Task Login_WithRegisteredPasskey_SavesItsUsage()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        Assert.Null(Assert.Single(await GetPasskeysAsync(context)).LastUsedUtc);

        // Act
        await LoginWithPasskeyAsync(CreateClient(context, out _), authenticator);

        // Assert
        var passkey = Assert.Single(await GetPasskeysAsync(context));
        Assert.NotNull(passkey.LastUsedUtc);
        Assert.Equal(1u, passkey.SignCount);
    }

    [Fact]
    public async Task Login_WithUserNameNoLongerAllowed_SignsIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        // Updating this user would now fail, which must not prevent them from logging in, as with a password.
        await context.UsingTenantScopeAsync(scope =>
        {
            scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value.User.AllowedUserNameCharacters = "z";

            return Task.CompletedTask;
        });

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithDisabledUser_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName) as User;

            user.IsEnabled = false;

            await userManager.UpdateAsync(user);
        });

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithLockedOutUser_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName);

            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));
        });

        var client = CreateClient(context, out var cookies);

        // Act
        var response = await LoginWithPasskeyAsync(client, authenticator);

        // Assert
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Login_WithLocalLoginDisabled_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");

        var client = CreateClient(context, out var cookies);
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Login", TestContext.Current.CancellationToken));

        var options = await PostAsync(client, "OrchardCore.Users/Passkey/RequestOptions", token, []);
        var credential = authenticator.CreateAssertion(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin);

        await DisableLocalLoginAsync(context);

        // Act
        var optionsResponse = await PostAsync(client, "OrchardCore.Users/Passkey/RequestOptions", token, []);
        var response = await PostAsync(client, "Passkeys/Login", token, new() { ["Credential"] = credential });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, optionsResponse.StatusCode);
        Assert.Contains("/Login", response.Headers.Location.ToString());
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task Create_WithMaximumNumberOfPasskeys_DoesNotAddPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);

        // The options are requested before the limit is reached, to also check that the credential is refused afterwards.
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));
        var options = await PostAsync(client, "OrchardCore.Users/Passkey/CreationOptions", token, []);
        var credential = authenticator.CreateAttestation(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName) as User;

            for (var i = 0; i < PasskeyService.MaxPasskeyCount; i++)
            {
                user.Passkeys.Add(new UserPasskey
                {
                    CredentialId = BitConverter.GetBytes(i),
                    PublicKey = [],
                    Name = $"Passkey {i}",
                    CreatedUtc = DateTime.UtcNow,
                });
            }

            await userManager.UpdateAsync(user);
        });

        // Act
        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);
        var optionsResponse = await PostAsync(client, "OrchardCore.Users/Passkey/CreationOptions", token, []);
        await PostAsync(client, "OrchardCore.Users/Passkey/Create", token, new() { ["Credential"] = credential });

        // Assert
        Assert.DoesNotContain("data-passkey-register", page);
        Assert.Equal(HttpStatusCode.BadRequest, optionsResponse.StatusCode);
        Assert.Equal(PasskeyService.MaxPasskeyCount, (await GetPasskeysAsync(context)).Count);
    }

    [Fact]
    public async Task Security_WithLocalLoginDisabled_DoesNotOfferPasskeys()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");
        await DisableLocalLoginAsync(context);

        // Act
        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);
        var optionsResponse = await PostAsync(client, "OrchardCore.Users/Passkey/CreationOptions", AntiForgeryHelper.ExtractAntiForgeryToken(page), []);

        // Assert
        Assert.DoesNotContain("data-passkey-register", page);
        Assert.Contains("My laptop", page);
        Assert.Equal(HttpStatusCode.BadRequest, optionsResponse.StatusCode);
    }

    [Fact]
    public async Task Security_WithLocalLoginDisabledAndTwoFactorMethod_OffersPasskeys()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);

        await LoginWithPasswordAsync(client);
        await DisableLocalLoginAsync(context);

        // Act
        var page = await client.GetStringAsync("Security", TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("data-passkey-register", page);
    }

    [Fact]
    public async Task Rename_WithValidName_RenamesPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");

        // Act
        await RenamePasskeyAsync(client, System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId), "  My phone ");

        // Assert
        Assert.Equal("My phone", Assert.Single(await GetPasskeysAsync(context)).Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A name that is far too long to be a sensible name for a passkey, and that goes well over the limit of the name")]
    public async Task Rename_WithInvalidName_KeepsName(string name)
    {
        // Arrange
        var context = await GetSiteContextAsync();
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");

        // Act
        await RenamePasskeyAsync(client, System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId), name);

        // Assert
        Assert.Equal("My laptop", Assert.Single(await GetPasskeysAsync(context)).Name);
    }

    [Fact]
    public async Task Delete_WithPasskeyOfAnotherUser_DoesNotRemovePasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var adminClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(adminClient);
        await RegisterPasskeyAsync(adminClient, authenticator, "My laptop");

        await CreateUserAsync(context, OtherUserName);

        var client = CreateClient(context, out _);
        await LoginWithPasswordAsync(client, OtherUserName);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        // Act
        await PostAsync(client, "OrchardCore.Users/Passkey/Delete", token, new()
        {
            ["Id"] = System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId),
        });

        // Assert
        Assert.Single(await GetPasskeysAsync(context));
    }

    [Fact]
    public async Task Delete_WithLastPasskeyAndTwoFactorEnabled_DisablesTwoFactor()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        // Act
        await PostAsync(client, "OrchardCore.Users/Passkey/Delete", token, new()
        {
            ["Id"] = System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId),
        });

        // Assert
        Assert.Empty(await GetPasskeysAsync(context));
        Assert.False(await IsTwoFactorEnabledAsync(context));
    }

    [Fact]
    public async Task Delete_WithOnlyMethodOfRequiredTwoFactor_KeepsPasskey()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);
        using var authenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);
        await SetTwoFactorLoginSettingsAsync(context, new TwoFactorLoginSettings { RequireTwoFactorAuthentication = true });

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        // Act
        await PostAsync(client, "OrchardCore.Users/Passkey/Delete", token, new()
        {
            ["Id"] = System.Buffers.Text.Base64Url.EncodeToString(authenticator.CredentialId),
        });

        // Assert
        Assert.Single(await GetPasskeysAsync(context));
        Assert.True(await IsTwoFactorEnabledAsync(context));
    }

    [Fact]
    public async Task RemoveAll_AsAdministrator_RemovesPasskeysAndDisablesTwoFactor()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        var client = CreateClient(context, out _);
        using var firstAuthenticator = new TestPasskeyAuthenticator();
        using var secondAuthenticator = new TestPasskeyAuthenticator();

        await LoginWithPasswordAsync(client);
        await RegisterPasskeyAsync(client, firstAuthenticator, "First");
        await RegisterPasskeyAsync(client, secondAuthenticator, "Second");
        await EnableTwoFactorAuthenticationAsync(context);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        // Act
        var response = await PostAsync(client, $"Admin/OrchardCore.Users/AdminPasskey/RemoveAll/{await GetUserIdAsync(context)}", token, []);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(await GetPasskeysAsync(context));
        Assert.False(await IsTwoFactorEnabledAsync(context));

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            Assert.Null(await userManager.FindByPasskeyIdAsync(firstAuthenticator.CredentialId));
            Assert.Null(await userManager.FindByPasskeyIdAsync(secondAuthenticator.CredentialId));
        });
    }

    [Fact]
    public async Task RemoveAll_WithoutPermission_KeepsPasskeys()
    {
        // Arrange
        var context = await GetSiteContextAsync();
        using var authenticator = new TestPasskeyAuthenticator();

        var adminClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(adminClient);
        await RegisterPasskeyAsync(adminClient, authenticator, "My laptop");

        await CreateUserAsync(context, OtherUserName);

        var client = CreateClient(context, out _);
        await LoginWithPasswordAsync(client, OtherUserName);

        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        // By default, the test site grants all the permissions.
        var permissionsContextKey = Guid.NewGuid().ToString();
        SiteStartup.PermissionsContexts.TryAdd(permissionsContextKey, new PermissionsContext { UsePermissionsContext = true });
        client.DefaultRequestHeaders.Add("PermissionsContext", permissionsContextKey);

        // Act
        await PostAsync(client, $"Admin/OrchardCore.Users/AdminPasskey/RemoveAll/{await GetUserIdAsync(context)}", token, []);

        // Assert
        Assert.Single(await GetPasskeysAsync(context));
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_WithInvalidPasskey_CountsFailedAttempt()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);

        var client = CreateClient(context, out var cookies);

        // Act
        await LoginWithPasskeyAsSecondFactorAsync(client, authenticator, tamper: true);

        // Assert
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            Assert.Equal(1, await userManager.GetAccessFailedCountAsync(await userManager.FindByNameAsync(AdminUserName)));
        });
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_WithPasskeyOfAnotherUser_DoesNotSignIn()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var adminAuthenticator = new TestPasskeyAuthenticator();
        using var otherAuthenticator = new TestPasskeyAuthenticator();

        var adminClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(adminClient);
        await RegisterPasskeyAsync(adminClient, adminAuthenticator, "Admin");
        await EnableTwoFactorAuthenticationAsync(context);

        await CreateUserAsync(context, OtherUserName);

        var otherClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(otherClient, OtherUserName);
        await RegisterPasskeyAsync(otherClient, otherAuthenticator, "Other");

        var client = CreateClient(context, out var cookies);

        // Act
        await LoginWithPasskeyAsSecondFactorAsync(client, otherAuthenticator);

        // Assert
        Assert.Null(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_WithRememberDevice_SkipsSecondFactorNextTime()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);
        await SetTwoFactorLoginSettingsAsync(context, new TwoFactorLoginSettings { AllowRememberClientTwoFactorAuthentication = true });

        var client = CreateClient(context, out var cookies);

        await LoginWithPasskeyAsSecondFactorAsync(client, authenticator, rememberDevice: true);

        var authenticationCookie = cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName];
        Assert.NotNull(authenticationCookie);
        authenticationCookie.Expired = true;

        // Act
        var response = await LoginWithPasswordAsync(client);

        // Assert
        Assert.DoesNotContain("LoginWithTwoFactorAuthentication", response.Headers.Location.ToString());
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task LoginWithRecoveryCode_WithPasskeyAsTwoFactorMethod_SignsIn()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);

        string recoveryCode = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(await userManager.FindByNameAsync(AdminUserName), 2);

            recoveryCode = codes.First();
        });

        var client = CreateClient(context, out var cookies);

        await LoginWithPasswordAsync(client);

        var page = await client.GetStringAsync("LoginWithRecoveryCode", TestContext.Current.CancellationToken);

        // Act
        var response = await PostAsync(client, "LoginWithRecoveryCode", AntiForgeryHelper.ExtractAntiForgeryToken(page), new()
        {
            ["RecoveryCode"] = recoveryCode,
        });

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(cookies.GetCookies(client.BaseAddress)["orchauth_" + context.TenantName]);
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_WithPreferredAuthenticatorApp_AsksForCode()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.AuthenticatorApp);
        using var authenticator = new TestPasskeyAuthenticator();

        var registrationClient = CreateClient(context, out _);
        await LoginWithPasswordAsync(registrationClient);
        await RegisterPasskeyAsync(registrationClient, authenticator, "My laptop");
        await EnableTwoFactorAuthenticationAsync(context);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName) as User;

            await userManager.ResetAuthenticatorKeyAsync(user);

            user.Put(new TwoFactorPreference { DefaultProvider = TokenOptions.DefaultAuthenticatorProvider });

            await userManager.UpdateAsync(user);
        });

        var client = CreateClient(context, out _);
        var loginResponse = await LoginWithPasswordAsync(client);

        // Act
        var page = await client.GetStringAsync(loginResponse.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("type=\"text\"", page);
        Assert.DoesNotContain("passkeys.", page);
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

    private static async Task<HttpResponseMessage> LoginWithPasswordAsync(HttpClient client, string userName = AdminUserName)
    {
        var loginPage = await client.GetStringAsync("Login", TestContext.Current.CancellationToken);

        var response = await PostAsync(client, "Login", AntiForgeryHelper.ExtractAntiForgeryToken(loginPage), new()
        {
            [$"{nameof(LoginForm)}.{nameof(LoginViewModel.UserName)}"] = userName,
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

    private static async Task LoginWithPasskeyAsSecondFactorAsync(HttpClient client, TestPasskeyAuthenticator authenticator, bool tamper = false, bool rememberDevice = false)
    {
        var loginResponse = await LoginWithPasswordAsync(client);
        Assert.Contains("LoginWithTwoFactorAuthentication", loginResponse.Headers.Location.ToString());

        var page = await client.GetStringAsync(loginResponse.Headers.Location, TestContext.Current.CancellationToken);
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(page);

        var options = await PostAsync(client, "OrchardCore.Users/Passkey/TwoFactorRequestOptions", token, []);
        options.EnsureSuccessStatusCode();

        var credential = authenticator.CreateAssertion(await options.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Origin, tamper);

        await PostAsync(client, "LoginWithTwoFactorAuthentication", token, new()
        {
            ["VerificationCode"] = credential,
            ["CurrentProvider"] = UserConstants.PasskeyTwoFactorProvider,
            ["RememberMe"] = "false",
            ["RememberDevice"] = rememberDevice ? "true" : "false",
        });
    }

    private static async Task RenamePasskeyAsync(HttpClient client, string id, string name)
    {
        var token = AntiForgeryHelper.ExtractAntiForgeryToken(await client.GetStringAsync("Security", TestContext.Current.CancellationToken));

        await PostAsync(client, "OrchardCore.Users/Passkey/Rename", token, new()
        {
            ["Id"] = id,
            ["Name"] = name,
        });
    }

    private static Task CreateUserAsync(SiteContext context, string userName)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var result = await userManager.CreateAsync(new User
            {
                UserName = userName,
                Email = $"{userName}@test.com",
                EmailConfirmed = true,
            }, AdminPassword);

            Assert.True(result.Succeeded);
        });

    private static async Task<string> GetUserIdAsync(SiteContext context)
    {
        string userId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            userId = await userManager.GetUserIdAsync(await userManager.FindByNameAsync(AdminUserName));
        });

        return userId;
    }

    private static async Task<bool> IsTwoFactorEnabledAsync(SiteContext context)
    {
        var isEnabled = false;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            isEnabled = await userManager.GetTwoFactorEnabledAsync(await userManager.FindByNameAsync(AdminUserName));
        });

        return isEnabled;
    }

    private static Task DisableLocalLoginAsync(SiteContext context)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            site.Put(new LoginSettings { DisableLocalLogin = true });

            await siteService.UpdateSiteSettingsAsync(site);
        });

    private static Task SetTwoFactorLoginSettingsAsync(SiteContext context, TwoFactorLoginSettings settings)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            site.Put(settings);

            await siteService.UpdateSiteSettingsAsync(site);
        });

    private static Task EnableTwoFactorAuthenticationAsync(SiteContext context)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            await userManager.SetTwoFactorEnabledAsync(await userManager.FindByNameAsync(AdminUserName), true);
        });

    private static Task SetUserVerificationRequirementAsync(SiteContext context, string userVerificationRequirement)
        => context.UsingTenantScopeAsync(scope =>
        {
            scope.ServiceProvider.GetRequiredService<IOptions<IdentityPasskeyOptions>>().Value.UserVerificationRequirement = userVerificationRequirement;

            return Task.CompletedTask;
        });

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
