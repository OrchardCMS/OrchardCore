using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.RemoteManagement;

public class RemoteManagementIdentityEndpointsTests
{
    private static readonly Permission s_granted = new("Granted");
    private static readonly Permission s_denied = new("Denied");

    [Fact]
    public async Task GetAsync_UserToken_ReturnsIdentityScopesAndEffectivePermissions()
    {
        var identity = new ClaimsIdentity("Bearer", OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, "user-id"));
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "alice"));
        var principal = new ClaimsPrincipal(identity);
        principal.SetPresenters("orchardcore-cli");
        principal.SetScopes(RemoteManagementConstants.ManagementScope, "openid");
        var context = new DefaultHttpContext { User = principal };

        var response = await GetAsync(context);

        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Equal("TenantA", response.Tenant);
        Assert.Equal("user", response.Type);
        Assert.Equal("user-id", response.Subject);
        Assert.Equal("alice", response.Name);
        Assert.Equal("orchardcore-cli", response.ClientId);
        Assert.Equal(["openid", RemoteManagementConstants.ManagementScope], response.Scopes);
        Assert.Equal([s_granted.Name], response.Permissions);
    }

    [Fact]
    public async Task GetAsync_ClientCredentialsToken_ReportsAClient()
    {
        var identity = new ClaimsIdentity("Bearer", OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, "automation"));
        var principal = new ClaimsPrincipal(identity);
        principal.SetPresenters("automation");

        var response = await GetAsync(new DefaultHttpContext { User = principal });

        Assert.Equal("client", response.Type);
        Assert.Equal("automation", response.ClientId);
        Assert.Null(response.Name);
    }

    private static async Task<RemoteManagementIdentityResponse> GetAsync(HttpContext context)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == s_granted.Name)
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());
        var permissions = new Mock<IPermissionService>();
        permissions.Setup(service => service.GetPermissionsAsync()).ReturnsAsync([s_denied, s_granted, s_granted]);

        var result = await RemoteManagementIdentityEndpoints.GetAsync(context, authorization.Object, permissions.Object, new ShellSettings { Name = "TenantA" });

        return Assert.IsType<Ok<RemoteManagementIdentityResponse>>(result).Value;
    }
}
