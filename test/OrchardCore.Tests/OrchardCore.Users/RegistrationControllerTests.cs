using OrchardCore.Entities;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.OrchardCore.Users;

public class RegistrationControllerTests
{
    [Fact]
    public async Task Register_WhenAllowed_RegisterUser()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
        });

        var responseFromGet = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet.IsSuccessStatusCode);

        // Act
        var model = new RegisterViewModel()
        {
            UserName = "test",
            Email = "test@orchardcore.com",
            Password = "test@OC!123",
            ConfirmPassword = "test@OC!123",
        };

        var response = await context.Client.SendAsync(await CreateRequestMessageAsync(model, responseFromGet));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(model.UserName) as User;

            Assert.NotNull(user);
            Assert.Equal(model.Email, user.Email);
        });
    }

    [Fact]
    public async Task Register_WhenNotAllowed_ReturnNotFound()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.NoRegistration,
        });

        // Act
        var response = await context.Client.GetAsync("Register");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenFeatureIsNotEnable_ReturnNotFound()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
        }, enableRegistrationFeature: false);

        // Act
        var response = await context.Client.GetAsync("Register");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenRequireUniqueEmailIsTrue_PreventRegisteringMultipleUsersWithTheSameEmails()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
        });

        var responseFromGet = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet.IsSuccessStatusCode);

        var emailAddress = "test@orchardcore.com";

        var requestForPost = await CreateRequestMessageAsync(new RegisterViewModel()
        {
            UserName = "test1",
            Email = emailAddress,
            Password = "test1@OC!123",
            ConfirmPassword = "test1@OC!123",
        }, responseFromGet);

        // Act
        var responseFromPost1 = await context.Client.SendAsync(requestForPost);

        Assert.Equal(HttpStatusCode.Redirect, responseFromPost1.StatusCode);

        var responseFromGet2 = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet2.IsSuccessStatusCode);

        var requestForPost2 = await CreateRequestMessageAsync(new RegisterViewModel()
        {
            UserName = "test2",
            Email = emailAddress,
            Password = "test2@OC!123",
            ConfirmPassword = "test2@OC!123",
        }, responseFromGet);

        var responseFromPost2 = await context.Client.SendAsync(requestForPost2);

        // Assert
        Assert.True(responseFromPost2.IsSuccessStatusCode);
        Assert.Contains("A user with the same email address already exists.", await responseFromPost2.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_WhenRequireUniqueEmailIsFalse_AllowRegisteringMultipleUsersWithTheSameEmails()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
        }, enableRegistrationFeature: true, requireUniqueEmail: false);

        // Register First User
        var responseFromGet = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet.IsSuccessStatusCode);
        var emailAddress = "test@orchardcore.com";

        var requestForPost = await CreateRequestMessageAsync(new RegisterViewModel()
        {
            UserName = "test1",
            Email = emailAddress,
            Password = "test1@OC!123",
            ConfirmPassword = "test1@OC!123",
        }, responseFromGet);

        var responseFromPost = await context.Client.SendAsync(requestForPost);

        Assert.Equal(HttpStatusCode.Redirect, responseFromPost.StatusCode);

        // Register Second User
        var responseFromGet2 = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet2.IsSuccessStatusCode);

        var requestForPost2 = await CreateRequestMessageAsync(new RegisterViewModel()
        {
            UserName = "test2",
            Email = emailAddress,
            Password = "test2@OC!123",
            ConfirmPassword = "test2@OC!123",
        }, responseFromGet);

        var responseFromPost2 = await context.Client.SendAsync(requestForPost2);

        Assert.Equal(HttpStatusCode.Redirect, responseFromPost2.StatusCode);

        var body = await responseFromPost2.Content.ReadAsStringAsync();

        Assert.DoesNotContain("A user with the same email address already exists.", body);
    }

    [Fact]
    public async Task Register_WhenModeration_RedirectToRegistrationPending()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
            UsersAreModerated = true,
        });

        var responseFromGet = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet.IsSuccessStatusCode);

        // Act
        var model = new RegisterViewModel()
        {
            UserName = "ModerateMe",
            Email = "ModerateMe@orchardcore.com",
            Password = "ModerateMe@OC!123",
            ConfirmPassword = "ModerateMe@OC!123",
        };

        var responseFromPost = await context.Client.SendAsync(await CreateRequestMessageAsync(model, responseFromGet));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, responseFromPost.StatusCode);
        Assert.Equal($"/{context.TenantName}/{nameof(RegistrationController.RegistrationPending)}", responseFromPost.Headers.Location.ToString());

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(model.UserName) as User;

            Assert.NotNull(user);
            Assert.Equal(model.Email, user.Email);
            Assert.False(user.IsEnabled);
        });
    }

    [Fact]
    public async Task Register_WhenRequireEmailConfirmation_RedirectToConfirmEmailSent()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings()
        {
            UsersCanRegister = UserRegistrationType.AllowRegistration,
            UsersMustValidateEmail = true,
        });

        var responseFromGet = await context.Client.GetAsync("Register");

        Assert.True(responseFromGet.IsSuccessStatusCode);

        // Act
        var model = new RegisterViewModel()
        {
            UserName = "ConfirmMe",
            Email = "ConfirmMe@orchardcore.com",
            Password = "ConfirmMe@OC!123",
            ConfirmPassword = "ConfirmMe@OC!123",
        };

        var requestForPost = await CreateRequestMessageAsync(model, responseFromGet);

        var responseFromPost = await context.Client.SendAsync(requestForPost);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, responseFromPost.StatusCode);
        Assert.Equal($"/{context.TenantName}/ConfirmEmailSent", responseFromPost.Headers.Location.ToString());

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(model.UserName) as User;

            Assert.NotNull(user);
            Assert.Equal(model.Email, user.Email);
            Assert.False(user.EmailConfirmed);
        });
    }

    private static async Task<HttpRequestMessage> CreateRequestMessageAsync(RegisterViewModel model, HttpResponseMessage response)
    {
        var data = new Dictionary<string, string>
        {
            {"__RequestVerificationToken", await AntiForgeryHelper.ExtractAntiForgeryToken(response) },
            {$"{nameof(RegisterUserForm)}.{nameof(model.UserName)}", model.UserName},
            {$"{nameof(RegisterUserForm)}.{nameof(model.Email)}", model.Email},
            {$"{nameof(RegisterUserForm)}.{nameof(model.Password)}", model.Password},
            {$"{nameof(RegisterUserForm)}.{nameof(model.ConfirmPassword)}", model.ConfirmPassword},
        };

        return PostRequestHelper.CreateMessageWithCookies("Register", data, response);
    }

    private static async Task<SiteContext> GetSiteContextAsync(RegistrationSettings settings, bool enableRegistrationFeature = true, bool requireUniqueEmail = true)
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            if (!requireUniqueEmail)
            {
                var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();
                var recipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();
                var recipeCollections = await Task.WhenAll(
                    recipeHarvesters.Select(recipe => recipe.HarvestRecipesAsync()));

                var recipe = recipeCollections.SelectMany(recipeCollection => recipeCollection)
                    .FirstOrDefault(recipe => recipe.Name == "UserSettingsTest");

                var executionId = Guid.NewGuid().ToString("n");

                await recipeExecutor.ExecuteAsync(
                    executionId,
                    recipe,
                    new Dictionary<string, object>(),
                    CancellationToken.None);
            }

            if (enableRegistrationFeature)
            {
                var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

                var extensionInfo = extensionManager.GetExtension(UserConstants.Features.UserRegistration);

                await shellFeatureManager.EnableFeaturesAsync([new FeatureInfo(UserConstants.Features.UserRegistration, extensionInfo)], true);
            }

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();

            site.Put(settings);

            await siteService.UpdateSiteSettingsAsync(site);
        });

        return context;
    }
}
