using System.Text.Json;
using System.Text.Json.Nodes;
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
