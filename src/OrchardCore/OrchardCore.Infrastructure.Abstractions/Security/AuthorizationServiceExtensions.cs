using System.Security.Claims;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace Microsoft.AspNetCore.Authorization;

public static class AuthorizationServiceExtensions
{
    public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission)
    {
        return AuthorizeAsync(service, user, permission, null);
    }

    public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission, object resource)
    {
        if (user == null)
        {
            return Task.FromResult(false);
        }

        var task = service.AuthorizeAsync(user, resource, new PermissionRequirement(permission));

        if (task.IsCompletedSuccessfully)
        {
            return Task.FromResult(task.Result.Succeeded);
        }

        return Awaited(task);

        static async Task<bool> Awaited(Task<AuthorizationResult> task)
        {
            return (await task).Succeeded;
        }
    }
}
