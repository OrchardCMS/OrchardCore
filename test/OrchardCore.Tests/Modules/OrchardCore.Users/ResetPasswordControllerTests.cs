using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OrchardCore.Users;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class ResetPasswordControllerTests
{
    [Fact]
    public async Task ForgotPassword_RateLimitExceededReturnsTooManyRequests_Succeeds()
    {
        // Arrange
        var context = await GetSiteContextAsync();

        var forgotPasswordGet = await context.Client.GetAsync("ForgotPassword", TestContext.Current.CancellationToken);
        Assert.True(forgotPasswordGet.IsSuccessStatusCode);

        // Act
        HttpResponseMessage response = null;

        for (var i = 0; i < 6; i++)
        {
            response = await context.Client.SendAsync(await CreateForgotPasswordRequestMessageAsync("missing-user", forgotPasswordGet), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    private static async Task<HttpRequestMessage> CreateForgotPasswordRequestMessageAsync(string usernameOrEmail, HttpResponseMessage response)
    {
        var data = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(response) },
            { $"{nameof(ForgotPasswordForm)}.{nameof(ForgotPasswordForm.UsernameOrEmail)}", usernameOrEmail },
        };

        return HttpRequestHelper.CreatePostMessageWithCookies("ForgotPassword", data, response);
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
                    { "name", "settings" },
                    { nameof(ResetPasswordSettings), JsonSerializer.SerializeToNode(new ResetPasswordSettings
                    {
                        AllowResetPassword = true,
                    }) },
                },
                new JsonObject
                {
                    { "name", "feature" },
                    { "enable", new JsonArray(UserConstants.Features.ResetPassword, "OrchardCore.RateLimits") },
                },
            },
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }
}
