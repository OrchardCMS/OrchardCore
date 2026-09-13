using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Endpoints.Management;
using OrchardCore.Security;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OpenIdScopeManagementTests
{
    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageScopes")]
    public async Task Mutations_DeniedPermission_DoNotAccessScopeStore(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        var body = new OpenIdScopeMutationRequest { Name = "scope", DisplayName = "Scope" };
        var settings = new ShellSettings { Name = "Default" };
        foreach (var result in new[]
        {
            await OpenIdScopeManagementEndpoints.CreateAsync(http, Authorize(denied), manager.Object, settings, body),
            await OpenIdScopeManagementEndpoints.UpdateAsync(http, Authorize(denied), manager.Object, settings, "scope", body),
            await OpenIdScopeManagementEndpoints.DeleteAsync(http, Authorize(denied), manager.Object, "scope"),
        })
        {
            Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Mutations_PreserveIdentityAndPropertiesWithEquivalentRetries()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await features.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.OpenId.Management");
            await features.EnableFeaturesAsync([feature], force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        var request = new OpenIdScopeMutationRequest { Name = "api-scope", DisplayName = "Scope", Resources = ["api-2", "api-1", "api-1"] };
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdScopeManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            http.Request.PathBase = "/tenant";
            var created = Assert.IsType<Created<OpenIdScopeResponse>>(await OpenIdScopeManagementEndpoints.CreateAsync(http, Authorize(), manager,
                services.GetRequiredService<ShellSettings>(), request));
            Assert.Equal("/tenant/api/openid/scopes/by-name?name=api-scope", created.Location);
            Assert.Equal(["api-1", "api-2"], created.Value.Resources.Order(StringComparer.Ordinal));
            Assert.Equal(["api-1", "api-2"], (await manager.ListResourcesAsync(["api-scope"]).ToArrayAsync()).Order(StringComparer.Ordinal));
            id = created.Value.Id;
            var item = await manager.FindByNameAsync("api-scope");
            var descriptor = new OpenIdScopeDescriptor();
            await manager.PopulateAsync(descriptor, item);
            descriptor.Properties.Add("extension", JsonSerializer.SerializeToElement("keep-me"));
            await manager.UpdateAsync(item, descriptor);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdScopeManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            var settings = services.GetRequiredService<ShellSettings>();
            var retry = Assert.IsType<Ok<OpenIdScopeResponse>>(await OpenIdScopeManagementEndpoints.CreateAsync(http, Authorize(), manager, settings, request));
            Assert.Equal(id, retry.Value.Id);
            var replacement = new OpenIdScopeMutationRequest { Name = "api-scope", DisplayName = "Updated" };
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdScopeManagementEndpoints.CreateAsync(http, Authorize(), manager, settings, replacement)).StatusCode);
            Assert.Equal("Scope", await manager.GetDisplayNameAsync(await manager.FindByNameAsync("api-scope")));
            var updated = Assert.IsType<Ok<OpenIdScopeResponse>>(await OpenIdScopeManagementEndpoints.UpdateAsync(http, Authorize(), manager, settings, "api-scope", replacement));
            Assert.Equal(id, updated.Value.Id);
            Assert.Empty(updated.Value.Resources);
            Assert.Empty(await manager.ListResourcesAsync(["api-scope"]).ToArrayAsync());
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdScopeManagementEndpoints.UpdateAsync(http, Authorize(), manager, settings, "other", replacement)).StatusCode);
            var item = await manager.FindByNameAsync("api-scope");
            Assert.Equal("Updated", await manager.GetDisplayNameAsync(item));
            Assert.Equal("keep-me", (await manager.GetPropertiesAsync(item))["extension"].GetString());
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdScopeManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            Assert.IsType<NoContent>(await OpenIdScopeManagementEndpoints.DeleteAsync(http, Authorize(), manager, "api-scope"));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdScopeManager>();
            Assert.Null(await manager.FindByNameAsync("api-scope"));
            Assert.IsType<NoContent>(await OpenIdScopeManagementEndpoints.DeleteAsync(new DefaultHttpContext { RequestServices = services }, Authorize(), manager, "api-scope"));
        });
    }

    private static IAuthorizationService Authorize(string denied = null)
    {
        var auth = new Mock<IAuthorizationService>();
        auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == denied)
                    ? AuthorizationResult.Failed() : AuthorizationResult.Success());
        return auth.Object;
    }
}
