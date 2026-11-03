using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class TwoFactorAuthenticationControllerTests
{
    [Fact]
    public async Task LoginWithTwoFactorAuthentication_RateLimitExceededReturnsTooManyRequests_Succeeds()
    {
        // Arrange
        var context = await GetSiteContextAsync();

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 6; i++)
        {
            response = await context.Client.PostAsync("LoginWithTwoFactorAuthentication", new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task LoginWithRecoveryCode_RateLimitExceededReturnsTooManyRequests_Succeeds()
    {
        // Arrange
        var context = await GetSiteContextAsync();

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 4; i++)
        {
            response = await context.Client.PostAsync("LoginWithRecoveryCode", new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task LoginWithTwoFactorAuthentication_UsersSharingAnIpAddress_HaveSeparateBudgets()
    {
        // Arrange
        var context = await GetSiteContextAsync();

        var firstUserCookie = await CreateTwoFactorUserCookieAsync(context, "first-user-id");
        var secondUserCookie = await CreateTwoFactorUserCookieAsync(context, "second-user-id");

        // Act
        HttpResponseMessage firstUserResponse = null;

        for (var i = 0; i < 6; i++)
        {
            firstUserResponse = await PostWithCookieAsync(context, "LoginWithTwoFactorAuthentication", firstUserCookie);
        }

        var secondUserResponse = await PostWithCookieAsync(context, "LoginWithTwoFactorAuthentication", secondUserCookie);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, firstUserResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, secondUserResponse.StatusCode);
    }

    private static async Task<string> CreateTwoFactorUserCookieAsync(SiteContext context, string userId)
    {
        string cookie = null;

        await context.UsingTenantScopeAsync(scope =>
        {
            // Mirrors the cookie that the sign-in manager issues once the password check succeeded.
            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.TwoFactorUserIdScheme);

            var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, userId));

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) },
                IdentityConstants.TwoFactorUserIdScheme);

            cookie = $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}";

            return Task.CompletedTask;
        });

        return cookie;
    }

    private static Task<HttpResponseMessage> PostWithCookieAsync(SiteContext context, string path, string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent([]),
        };

        request.Headers.Add("Cookie", cookie);

        return context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<SiteContext> GetSiteContextAsync()
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    { "name", "feature" },
                    { "enable", new JsonArray(UserConstants.Features.AuthenticatorApp, "OrchardCore.RateLimits") },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }
}
