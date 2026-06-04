using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class TwoFactorSendCodeEndpointTests
{
    [Fact]
    public async Task EmailSendCode_WhenRateLimitExceeded_ReturnsTooManyRequests()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.EmailAuthenticator);

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 3; i++)
        {
            response = await context.Client.PostAsync("TwoFactor-Authenticator/EmailSendCode", new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task SmsSendCode_WhenRateLimitExceeded_ReturnsTooManyRequests()
    {
        // Arrange
        var context = await GetSiteContextAsync(UserConstants.Features.SmsAuthenticator);

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 3; i++)
        {
            response = await context.Client.PostAsync("TwoFactor-Authenticator/SmsSendCode", new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    private static async Task<SiteContext> GetSiteContextAsync(string featureId)
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
                    { "enable", new JsonArray(featureId, "OrchardCore.RateLimits") },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }
}
