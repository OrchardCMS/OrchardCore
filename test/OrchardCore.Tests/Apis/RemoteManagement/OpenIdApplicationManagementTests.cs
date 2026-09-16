using System.Security.Claims;
using System.Security.Cryptography;
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

public class OpenIdApplicationManagementTests
{
    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageApplications")]
    public async Task Mutations_DeniedPermission_DoNotAccessStores(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var scopes = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        foreach (var result in new[]
        {
            await OpenIdApplicationManagementEndpoints.CreateAsync(http, Authorize(denied), manager.Object, scopes.Object, Request()),
            await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(denied), manager.Object, scopes.Object, "managed-client", Request()),
            await OpenIdApplicationManagementEndpoints.DeleteAsync(http, Authorize(denied), manager.Object, "managed-client"),
        })
        {
            Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }
        manager.VerifyNoOtherCalls();
        scopes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Mutations_PreserveIdentitySecretsAndPrivateProperties_AndReplaceGrants()
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
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var scopes = services.GetRequiredService<IOpenIdScopeManager>();
            await scopes.CreateAsync(new OpenIdScopeDescriptor { Name = "managed-api", DisplayName = "API" });
            var http = new DefaultHttpContext { RequestServices = services };
            http.Request.PathBase = "/tenant";
            var request = Request(secret);
            var created = Assert.IsType<Created<OpenIdApplicationResponse>>(await OpenIdApplicationManagementEndpoints.CreateAsync(http, Authorize(), manager, scopes, request));
            Assert.Equal("/tenant/api/openid/applications/by-client-id?clientId=managed-client", created.Location);
            id = created.Value.Id;
            Assert.Equal(["Editor"], created.Value.Roles);
            Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, created.Value.Permissions);
            Assert.False(JsonSerializer.Serialize(created.Value).Contains(secret, StringComparison.Ordinal));
            var application = await manager.FindByClientIdAsync("managed-client");
            Assert.True(await manager.ValidateClientSecretAsync(application, secret));
            var descriptor = new OpenIdApplicationDescriptor();
            await manager.PopulateAsync(descriptor, application);
            descriptor.Properties.Add("extension", JsonSerializer.SerializeToElement("preserve-me"));
            descriptor.Permissions.Add("custom-permission");
            descriptor.Requirements.Add("custom-requirement");
            await manager.UpdateAsync(application, descriptor);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var scopes = services.GetRequiredService<IOpenIdScopeManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            var retry = Assert.IsType<Ok<OpenIdApplicationResponse>>(await OpenIdApplicationManagementEndpoints.CreateAsync(http, Authorize(), manager, scopes, Request(secret)));
            Assert.Equal(id, retry.Value.Id);
            Assert.Equal(409, Status(await OpenIdApplicationManagementEndpoints.CreateAsync(http, Authorize(), manager, scopes, Request(secret, displayName: "Conflicting"))));
            Assert.Equal(409, Status(await OpenIdApplicationManagementEndpoints.CreateAsync(http, Authorize(), manager, scopes, Request(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))))));
            foreach (var invalid in new[]
            {
                Request(redirectUris: "not-an-absolute-uri"),
                Request(redirectUris: "https://example.com/callback#fragment"),
                Request(roles: ["missing-role"]),
                Request(scopes: ["missing-scope"]),
                Request(secret, clientType: "public"),
                Request(applicationType: "native"),
            })
            {
                Assert.Equal(400, Status(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "managed-client", invalid)));
                Assert.Equal("Application", await manager.GetDisplayNameAsync(await manager.FindByClientIdAsync("managed-client")));
            }
            Assert.Equal(400, Status(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "other", Request())));
            var replacement = Request(displayName: "Updated", roles: [], scopes: [], allowClientCredentials: false);
            var updated = Assert.IsType<Ok<OpenIdApplicationResponse>>(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "managed-client", replacement));
            Assert.Equal(id, updated.Value.Id);
            Assert.Empty(updated.Value.Roles);
            Assert.DoesNotContain(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, updated.Value.Permissions);
            Assert.DoesNotContain(OpenIddictConstants.Permissions.Endpoints.Token, updated.Value.Permissions);
            Assert.DoesNotContain("scp:managed-api", updated.Value.Permissions);
            Assert.Contains("custom-permission", updated.Value.Permissions);
            Assert.Contains("custom-requirement", updated.Value.Requirements);
            var application = await manager.FindByClientIdAsync("managed-client");
            Assert.True(await manager.ValidateClientSecretAsync(application, secret));
            Assert.Equal("preserve-me", (await manager.GetPropertiesAsync(application))["extension"].GetString());
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            var scopes = services.GetRequiredService<IOpenIdScopeManager>();
            var http = new DefaultHttpContext { RequestServices = services };
            var publicRequest = Request(clientType: "public", roles: [], scopes: [], allowClientCredentials: false);
            Assert.IsType<Ok<OpenIdApplicationResponse>>(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "managed-client", publicRequest));
            var application = await manager.FindByClientIdAsync("managed-client");
            var descriptor = new OpenIdApplicationDescriptor();
            await manager.PopulateAsync(descriptor, application);
            Assert.True(string.IsNullOrEmpty(descriptor.ClientSecret));
            Assert.Equal(400, Status(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "managed-client", Request())));
            Assert.IsType<Ok<OpenIdApplicationResponse>>(await OpenIdApplicationManagementEndpoints.UpdateAsync(http, Authorize(), manager, scopes, "managed-client", Request(secret)));
            Assert.True(await manager.ValidateClientSecretAsync(await manager.FindByClientIdAsync("managed-client"), secret));
            Assert.IsType<NoContent>(await OpenIdApplicationManagementEndpoints.DeleteAsync(http, Authorize(), manager, "managed-client"));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var manager = services.GetRequiredService<IOpenIdApplicationManager>();
            Assert.Null(await manager.FindByClientIdAsync("managed-client"));
            Assert.IsType<NoContent>(await OpenIdApplicationManagementEndpoints.DeleteAsync(new DefaultHttpContext { RequestServices = services }, Authorize(), manager, "managed-client"));
        });
    }

    private static OpenIdApplicationMutationRequest Request(string secret = null, string displayName = "Application",
        string clientType = "confidential", string applicationType = "web", string redirectUris = null,
        string[] roles = null, string[] scopes = null, bool allowClientCredentials = true) => new()
    {
        ClientId = "managed-client", DisplayName = displayName, ClientType = clientType, ApplicationType = applicationType,
        ClientSecret = secret, Roles = roles ?? ["Editor"], Scopes = scopes ?? ["managed-api"],
        AllowClientCredentialsFlow = allowClientCredentials, RedirectUris = redirectUris,
    };

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
