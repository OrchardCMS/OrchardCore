using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class CustomUserSettingsManagementTests
{
    [Fact]
    public async Task UpdateAsync_PersistsOnOwnerPreservesOtherSettingsAndRejectsIdentityChanges()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await features.EnableFeaturesAsync((await features.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id == "OrchardCore.Users.CustomUserSettings"), force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        const string typeName = "RemoteUserPreferences";
        string userId = null;
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(Permission.ClaimType, "ManageUsers")], "Test"));
        await context.UsingTenantScopeAsync(async scope =>
        {
            var definitions = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await definitions.AlterPartDefinitionAsync(typeName, part => { });
            await definitions.AlterTypeDefinitionAsync(typeName, type => type.Stereotype("CustomUserSettings").WithPart(typeName));
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = new User { UserName = "custom-settings-owner", Email = "owner@example.com" };
            user.Properties["Unrelated"] = new JsonObject { ["Value"] = "keep" };
            Assert.True((await users.CreateAsync(user)).Succeeded);
            userId = user.UserId;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<CustomUserSettingsManagementService>();
            var result = await service.UpdateAsync(principal, userId, typeName,
                new JsonObject { [typeName] = new JsonObject { ["Value"] = "saved", ["Other"] = "keep" } });
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeHttpResult)result).StatusCode ?? StatusCodes.Status200OK);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<CustomUserSettingsManagementService>();
            var denied = new ClaimsPrincipal(new ClaimsIdentity([new Claim(Permission.ClaimType, "ManageOwnCustomUserSettings_" + typeName)], "Test"));
            // SiteContext grants all permissions by default; explicitly model type-only access here.
            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                    requirements.OfType<PermissionRequirement>().All(requirement => requirement.Permission.Name == "ManageOwnCustomUserSettings_" + typeName)
                        ? AuthorizationResult.Success() : AuthorizationResult.Failed());
            var restricted = ActivatorUtilities.CreateInstance<CustomUserSettingsManagementService>(scope.ServiceProvider, authorization.Object);
            Assert.Equal(StatusCodes.Status403Forbidden, ((IStatusCodeHttpResult)await restricted.UpdateAsync(denied, userId, typeName, [])).StatusCode);
            var invalid = await service.UpdateAsync(principal, userId, typeName, new JsonObject { ["PasswordHash"] = "tampered" });
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeHttpResult)invalid).StatusCode);
            var valid = await service.UpdateAsync(principal, userId, typeName, new JsonObject { [typeName] = new JsonObject { ["Value"] = "updated" } });
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeHttpResult)valid).StatusCode ?? StatusCodes.Status200OK);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = Assert.IsType<User>(await users.FindByIdAsync(userId));
            Assert.Null(user.PasswordHash);
            Assert.Equal("keep", user.Properties["Unrelated"]?["Value"]?.GetValue<string>());
            Assert.Equal("updated", user.Properties[typeName]?[typeName]?["Value"]?.GetValue<string>());
            Assert.Equal("keep", user.Properties[typeName]?[typeName]?["Other"]?.GetValue<string>());
            var items = await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().Query<ContentItem>().ListAsync();
            Assert.DoesNotContain(items, item => item.ContentType == typeName);
            var service = scope.ServiceProvider.GetRequiredService<CustomUserSettingsManagementService>();
            var result = await service.GetAsync(principal, userId, typeName);
            var envelope = Assert.IsType<JsonObject>(((IValueHttpResult)result).Value);
            Assert.Null(envelope["ContentItemId"]);
            Assert.Null(envelope["Owner"]);
        });
    }
}
