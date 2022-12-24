using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;
using OrchardCore.Security;
using OrchardCore.Testing.Fakes;

namespace OrchardCore.Testing.Security;

public static class PermissionHandlerHelper
{
    public static AuthorizationHandlerContext CreateTestAuthorizationHandlerContext(Permission required, string[] allowed = null, bool authenticated = false)
    {
        var identity = authenticated
            ? new ClaimsIdentity("Testing")
            : new ClaimsIdentity();

        if (allowed != null)
        {
            foreach (var permissionName in allowed)
            {
                var permission = new Permission(permissionName);
                identity.AddClaim(permission);
            }

        }

        var principal = new ClaimsPrincipal(identity);

        return new AuthorizationHandlerContext(new[] { new PermissionRequirement(required) }, principal, null);
    }

    public static async Task SuccessAsync(this AuthorizationHandlerContext context, params string[] permissionNames)
    {
        var handler = new FakePermissionHandler(permissionNames);

        await handler.HandleAsync(context);
    }
}
