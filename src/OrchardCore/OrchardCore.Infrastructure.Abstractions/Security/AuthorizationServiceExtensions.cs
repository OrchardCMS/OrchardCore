using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission)
        {
            return AuthorizeAsync(service, user, permission, null);
        }

        public static async Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission, object resource)
        {
            if (user == null)
            {
                return false;
            }

            return (await service.AuthorizeAsync(user, resource, new PermissionRequirement(permission))).Succeeded;
        }
    }
}
