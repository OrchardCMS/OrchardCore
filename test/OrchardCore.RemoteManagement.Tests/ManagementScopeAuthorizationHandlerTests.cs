using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using Xunit;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.RemoteManagement.Tests;

public class ManagementScopeAuthorizationHandlerTests
{
    [Theory]
    [InlineData(true, RemoteManagementConstants.ManagementScope, false)]
    [InlineData(true, "other", true)]
    [InlineData(false, "other", false)]
    public async Task Handle_AccessToken_RequiresManagementScope(bool isToken, string scope, bool expectFailure)
    {
        var identity = new ClaimsIdentity("Test");
        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scope);
        if (isToken)
        {
            principal.SetTokenType(TokenTypeHints.AccessToken);
        }

        var requirement = new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await new ManagementScopeAuthorizationHandler().HandleAsync(context);

        Assert.Equal(expectFailure, context.HasFailed);
    }

    [Fact]
    public async Task Handle_OtherPermission_IsNotAffected()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity("Test"));
        principal.SetTokenType(TokenTypeHints.AccessToken);
        var context = new AuthorizationHandlerContext([new PermissionRequirement(new Permission("Other"))], principal, null);

        await new ManagementScopeAuthorizationHandler().HandleAsync(context);

        Assert.False(context.HasFailed);
    }
}
