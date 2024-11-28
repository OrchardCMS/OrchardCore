using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment.Services;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Tests.OrchardCore.Users;

public class AccountControllerTests
{
    [Fact]
    public async Task ExternalLoginSignIn_Test()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings(), true, true, true);

        // Act
        var model = new RegisterViewModel()
        {
            UserName = "TestUserName",
            Email = "test@orchardcore.com",
            Password = "test@OC!123",
            ConfirmPassword = "test@OC!123",
        };

        var responseFromGet = await context.Client.GetAsync("Register");
        responseFromGet.EnsureSuccessStatusCode();
        var response = await context.Client.SendAsync(await CreateRequestMessageAsync(model, responseFromGet));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/{context.TenantName}/", response.Headers.Location.ToString());

        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(model.UserName) as User;

            var externalClaims = new List<SerializableClaim>();
            var userRoles = await userManager.GetRolesAsync(user);

            var context = new UpdateUserContext(user, "TestLoginProvider", externalClaims, user.Properties.DeepClone() as JsonObject)
            {
                UserClaims = user.UserClaims,
                UserRoles = userRoles
            };

            context.UserProperties["Test"] = 111;
            Assert.NotEqual(user.Properties["Test"], context.UserProperties["Test"]);

            var scriptExternalLoginEventHandler = scope.ServiceProvider.GetServices<IExternalLoginEventHandler>()
                        .FirstOrDefault(x => x.GetType() == typeof(ScriptExternalLoginEventHandler)) as ScriptExternalLoginEventHandler;
            var loginSettings = new ExternalLoginSettings
            {
                UseScriptToSyncProperties = true,
                SyncPropertiesScript = """
                    if(!context.user.userClaims?.find(x=> x.claimType=="lastName" && claimValue=="Zhang")){
                        context.claimsToUpdate.push({claimType:"lastName",    claimValue:"Zhang"});
                    }
                    context.claimsToUpdate.push({claimType:"firstName",   claimValue:"Sam"});
                    context.claimsToUpdate.push({claimType:"displayName", claimValue:"Sam Zhang(CEO)"});
                    context.claimsToUpdate.push({claimType:"jobTitle",    claimValue:"CEO"});

                    if(!context.user.userRoles?.includes('Administrator')){
                        context.rolesToAdd.push("Administrator");
                    }

                    if(context.user.userProperties?.UserProfile?.UserProfile?.DisplayName?.Text!="Sam Zhang(CEO)")
                    {
                        context.propertiesToUpdate = {
                            "UserProfile": {
                                "UserProfile": {
                                    "DisplayName": {
                                        "Text": "Sam Zhang(CEO)"
                                    }
                                }
                            }
                        };
                    }
                    """
            };
            scriptExternalLoginEventHandler.UpdateUserInternal(context, loginSettings);

            if (await userManager.UpdateUserPropertiesAsync(user, context))
            {
                await userManager.UpdateAsync(user);
            }

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();
            var sam = await session.Query<User, UserByClaimIndex>()
                    .Where(claim => claim.ClaimType == "displayName" && claim.ClaimValue == "Sam Zhang(CEO)")
                    .FirstOrDefaultAsync();
            Assert.NotNull(sam);

            var claimsDict = sam.UserClaims.ToDictionary(claim => claim.ClaimType, claim => claim.ClaimValue);
            Assert.Equal("Sam", claimsDict["firstName"]);
            Assert.Equal("Zhang", claimsDict["lastName"]);
            Assert.Equal("CEO", claimsDict["jobTitle"]);
            Assert.Contains("Administrator", sam.RoleNames.FirstOrDefault());
            Assert.Equal("Sam Zhang(CEO)", sam.Properties.SelectNode("$.UserProfile.UserProfile.DisplayName.Text").ToString());

        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = await userManager.FindByNameAsync(model.UserName) as User;

            var externalClaims = new List<SerializableClaim>();
            var userRoles = await userManager.GetRolesAsync(user);

            var updateContext = new UpdateUserContext(user, "TestLoginProvider", externalClaims, user.Properties.DeepClone() as JsonObject)
            {
                UserClaims = user.UserClaims,
                UserRoles = userRoles,
            };

            var scriptExternalLoginEventHandler = scope.ServiceProvider.GetServices<IExternalLoginEventHandler>()
                      .FirstOrDefault(x => x.GetType() == typeof(ScriptExternalLoginEventHandler)) as ScriptExternalLoginEventHandler;
            var loginSettings = new ExternalLoginSettings
            {
                UseScriptToSyncProperties = true,
                SyncPropertiesScript = """
                    context.claimsToUpdate.push({claimType:"displayName", claimValue:"Sam Zhang"});
                    context.claimsToUpdate.push({claimType:"firstName",   claimValue:"Sam"});
                    context.claimsToUpdate.push({claimType:"lastName",    claimValue:"Zhang"});
                    context.claimsToRemove.push({claimType:"jobTitle",    claimValue:"CEO"});
                    context.rolesToRemove.push("Administrator");
                    context.propertiesToUpdate = {
                        "UserProfile": {
                            "UserProfile": {
                                "DisplayName": {
                                    "Text": "Sam Zhang"
                                }
                            }
                        }
                    };
                    """
            };
            scriptExternalLoginEventHandler.UpdateUserInternal(updateContext, loginSettings);

            if (await userManager.UpdateUserPropertiesAsync(user, updateContext))
            {
                await userManager.UpdateAsync(user);
            }

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();
            var userFromDb = await session.Query<User, UserByClaimIndex>()
                         .Where(claim => claim.ClaimType == "displayName" && claim.ClaimValue == "Sam Zhang")
                         .FirstOrDefaultAsync();

            Assert.DoesNotContain("Administrator", userFromDb.RoleNames);
            Assert.DoesNotContain(userFromDb.UserClaims, x => x.ClaimType == "jobTitle" && x.ClaimValue == "CEO");
            Assert.Equal("Sam Zhang", userFromDb.Properties.SelectNode("$.UserProfile.UserProfile.DisplayName.Text").ToString());
        });
    }

    [Fact]
    public async Task Register_WhenAllowed_RegisterUser()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings());

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
        var context = await GetSiteContextAsync(new RegistrationSettings(), false);

        // Act
        var response = await context.Client.GetAsync("Register");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenFeatureIsNotEnable_ReturnNotFound()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings(), enableRegistrationFeature: false);

        // Act
        var response = await context.Client.GetAsync("Register");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenRequireUniqueEmailIsTrue_PreventRegisteringMultipleUsersWithTheSameEmails()
    {
        // Arrange
        var context = await GetSiteContextAsync(new RegistrationSettings());

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
        var context = await GetSiteContextAsync(new RegistrationSettings(), enableRegistrationFeature: true, requireUniqueEmail: false);

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
        var context = await GetSiteContextAsync(new RegistrationSettings
        {
            UsersMustValidateEmail = true,
        }, true, true, false);

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

    private static async Task<SiteContext> GetSiteContextAsync(RegistrationSettings settings, bool enableRegistrationFeature = true, bool requireUniqueEmail = true, bool enableExternalAuthentication = false)
    {
        var context = new SiteContext();

        await context.InitializeAsync();

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                { "name", "settings" },
                { nameof(RegistrationSettings), JsonSerializer.SerializeToNode(settings)},
                { nameof(IdentitySettings), JsonSerializer.SerializeToNode(new IdentitySettings
                {
                    UserSettings = new IdentityUserSettings
                    {
                        RequireUniqueEmail = requireUniqueEmail,
                    }
                })},
            }
        };

        if (enableRegistrationFeature || enableExternalAuthentication)
        {
            var featureIds = new JsonArray();

            if (enableRegistrationFeature)
            {
                featureIds.Add(UserConstants.Features.UserRegistration);
            }

            if (enableExternalAuthentication)
            {
                featureIds.Add(UserConstants.Features.ExternalAuthentication);
            }

            recipeSteps.Add(new JsonObject
            {
                { "name", "feature" },
                { "enable", featureIds },
            });
        }

        var recipe = new JsonObject
        {
            ["steps"] = recipeSteps,
        };

        var t = recipe.ToJsonString();

        await RunRecipeAsync(context, recipe);

        return context;
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
