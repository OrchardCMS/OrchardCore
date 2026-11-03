using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using OrchardCore.Security;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Requires access tokens to carry the management scope, so a token issued
/// to a client for another purpose can't call management APIs for its user.
/// </summary>
internal sealed class ManagementScopeAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (requirement.Permission.Name == RemoteManagementPermissions.AccessRemoteManagement.Name &&
            !string.IsNullOrEmpty(context.User.GetTokenType()) &&
            !context.User.HasScope(RemoteManagementConstants.ManagementScope))
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
