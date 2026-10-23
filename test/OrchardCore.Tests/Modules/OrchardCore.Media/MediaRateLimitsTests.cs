using System.Text;
using System.Text.Json.Nodes;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaRateLimitsTests
{
    [Fact]
    public async Task GetMediaFile_BeyondGlobalPolicyLimit_IsNotRateLimited()
    {
        // Arrange
        var context = new SiteContext();

        await context.InitializeAsync();

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    { "name", "Feature" },
                    { "enable", new JsonArray("OrchardCore.Media", "OrchardCore.RateLimits") },
                },
                new JsonObject
                {
                    { "name", "CreateOrUpdateRateLimitPolicies" },
                    { "policies", new JsonArray
                        {
                            new JsonObject
                            {
                                { "PolicyId", "media-test-global-policy" },
                                { "Name", "Media Test Global Policy" },
                                { "IsEnabled", true },
                                { "Scope", "Global" },
                                { "Limiters", new JsonArray
                                    {
                                        new JsonObject
                                        {
                                            { "Id", "media-test-global-limiter" },
                                            { "Source", "FixedWindow" },
                                            { "Properties", new JsonObject
                                                {
                                                    { "FixedWindowRateLimiterData", new JsonObject
                                                        {
                                                            { "PermitLimit", 2 },
                                                            { "WindowSeconds", 60 },
                                                            { "QueueLimit", 0 },
                                                        }
                                                    },
                                                }
                                            },
                                        },
                                    }
                                },
                            },
                        }
                    },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var mediaFileStore = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("media-content"));
            await mediaFileStore.CreateFileFromStreamAsync("rate-limit-test.txt", stream, overwrite: true);
        });

        // Act & Assert
        // A page can reference many media files, so they must not consume the budget of the visitor.
        for (var i = 0; i < 5; i++)
        {
            var mediaResponse = await context.Client.GetAsync("media/rate-limit-test.txt", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, mediaResponse.StatusCode);
        }

        HttpResponseMessage pageResponse = null;

        for (var i = 0; i < 3; i++)
        {
            pageResponse = await context.Client.GetAsync("Login", TestContext.Current.CancellationToken);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, pageResponse.StatusCode);
    }
}
