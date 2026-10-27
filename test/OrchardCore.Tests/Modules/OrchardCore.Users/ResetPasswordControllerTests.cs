using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Email;
using OrchardCore.Email.Smtp.Services;
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

        // The password recovery allows 20 attempts per minute per IP address.
        for (var i = 0; i < 21; i++)
        {
            response = await context.Client.SendAsync(await CreateForgotPasswordRequestMessageAsync("missing-user", forgotPasswordGet), TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithinCooldown_SendsSingleEmailPerUser()
    {
        // Arrange
        var context = await GetSiteContextAsync(enableSmtp: true);

        string pickupDirectory = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var result = await userManager.CreateAsync(new User
            {
                UserName = "forgetful-user",
                Email = "forgetful-user@orchardcore.com",
                EmailConfirmed = true,
                IsEnabled = true,
            }, "Password1!");

            Assert.True(result.Succeeded);

            pickupDirectory = scope.ServiceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value.PickupDirectoryLocation;
        });

        var forgotPasswordGet = await context.Client.GetAsync("ForgotPassword", TestContext.Current.CancellationToken);
        Assert.True(forgotPasswordGet.IsSuccessStatusCode);

        // Act
        for (var i = 0; i < 3; i++)
        {
            var response = await context.Client.SendAsync(await CreateForgotPasswordRequestMessageAsync("forgetful-user", forgotPasswordGet), TestContext.Current.CancellationToken);

            // The confirmation page is shown every time, so that the cooldown doesn't reveal whether the account exists.
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.EndsWith("/ForgotPasswordConfirmation", response.Headers.Location.ToString());
        }

        // Assert
        Assert.Single(Directory.GetFiles(pickupDirectory, "*.eml"));
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

    private static async Task<SiteContext> GetSiteContextAsync(bool enableSmtp = false)
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

        if (enableSmtp)
        {
            var steps = recipe["steps"].AsArray();

            steps.Add(new JsonObject
            {
                { "name", "feature" },
                { "enable", new JsonArray("OrchardCore.Email.Smtp") },
            });

            steps.Add(new JsonObject
            {
                { "name", "settings" },
                { nameof(SmtpSettings), JsonSerializer.SerializeToNode(new SmtpSettings
                {
                    IsEnabled = true,
                    DefaultSender = "noreply@orchardcore.com",
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = "/",
                }) },
                { nameof(EmailSettings), JsonSerializer.SerializeToNode(new EmailSettings
                {
                    DefaultProviderName = SmtpEmailProvider.TechnicalName,
                }) },
            });
        }

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        return context;
    }
}
