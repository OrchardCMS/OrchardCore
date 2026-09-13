using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Endpoints.Management;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OpenIdApplicationCredentialTests
{
    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageApplications")]
    public async Task CredentialChanges_DeniedPermission_DoNotReadOrWriteApplication(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        Assert.Equal(403, Status(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(denied), manager.Object, "client")));
        Assert.Equal(403, Status(await OpenIdApplicationCredentialEndpoints.RevokeAsync(http, Authorize(denied), manager.Object, "client")));
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CredentialChanges_RetirePreviousSecrets_AndPreserveApplicationSettings()
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
        var originalSecret = RemoteManagementClientCredentials.GenerateSecret();
        string replacement = null;
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var descriptor = new OpenIdApplicationDescriptor
            {
                ClientId = "rotating-client", ClientSecret = originalSecret, DisplayName = "Automation",
                ClientType = OpenIddictConstants.ClientTypes.Confidential, ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            };
            descriptor.Permissions.UnionWith([OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, OpenIddictConstants.Permissions.Endpoints.Token, "scp:api"]);
            descriptor.Roles.Add("Editor");
            descriptor.Properties.Add("extension", JsonSerializer.SerializeToElement("preserve-me"));
            var application = await manager.CreateAsync(descriptor);
            id = await manager.GetPhysicalIdAsync(application);
            await manager.CreateAsync(new OpenIdApplicationDescriptor { ClientId = "public-client", ClientType = OpenIddictConstants.ClientTypes.Public });
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            Assert.Equal(400, Status(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "")));
            Assert.Equal(404, Status(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "missing")));
            Assert.IsType<NoContent>(await OpenIdApplicationCredentialEndpoints.RevokeAsync(http, Authorize(), manager, "missing"));
            Assert.Equal(400, Status(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "public-client")));
            Assert.Equal(400, Status(await OpenIdApplicationCredentialEndpoints.RevokeAsync(http, Authorize(), manager, "public-client")));
            var rotated = Assert.IsType<Ok<RemoteManagementClientCredentials>>(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "rotating-client"));
            Assert.Equal("no-store", http.Response.Headers.CacheControl);
            Assert.Equal("rotating-client", rotated.Value.ClientId);
            replacement = rotated.Value.ClientSecret;
            Assert.True(replacement != originalSecret);
            var application = await manager.FindByClientIdAsync("rotating-client");
            Assert.False(await manager.ValidateClientSecretAsync(application, originalSecret));
            Assert.True(await manager.ValidateClientSecretAsync(application, replacement));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            var repeated = Assert.IsType<Ok<RemoteManagementClientCredentials>>(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "rotating-client"));
            var application = await manager.FindByClientIdAsync("rotating-client");
            Assert.False(await manager.ValidateClientSecretAsync(application, replacement));
            replacement = repeated.Value.ClientSecret;
            Assert.True(await manager.ValidateClientSecretAsync(application, replacement));
            Assert.IsType<NoContent>(await OpenIdApplicationCredentialEndpoints.RevokeAsync(http, Authorize(), manager, "rotating-client"));
            application = await manager.FindByClientIdAsync("rotating-client");
            Assert.False(await manager.ValidateClientSecretAsync(application, replacement));
            Assert.True(await manager.HasClientTypeAsync(application, OpenIddictConstants.ClientTypes.Confidential));
            Assert.IsType<NoContent>(await OpenIdApplicationCredentialEndpoints.RevokeAsync(http, Authorize(), manager, "rotating-client"));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            var restored = Assert.IsType<Ok<RemoteManagementClientCredentials>>(await OpenIdApplicationCredentialEndpoints.RotateAsync(http, Authorize(), manager, "rotating-client"));
            var application = await manager.FindByClientIdAsync("rotating-client");
            Assert.True(await manager.ValidateClientSecretAsync(application, restored.Value.ClientSecret));
            Assert.False(await manager.ValidateClientSecretAsync(application, replacement));
            Assert.Equal(id, await manager.GetPhysicalIdAsync(application));
            Assert.Equal("Automation", await manager.GetDisplayNameAsync(application));
            Assert.Equal(["Editor"], await manager.GetRolesAsync(application));
            Assert.Equal("preserve-me", (await manager.GetPropertiesAsync(application))["extension"].GetString());
            Assert.Equal(new[] { "ept:token", "gt:client_credentials", "scp:api" }, (await manager.GetPermissionsAsync(application)).Order(StringComparer.Ordinal));
        });
    }

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

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
