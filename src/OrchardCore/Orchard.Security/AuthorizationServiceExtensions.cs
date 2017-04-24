using System.Security.Claims;
using System.Threading.Tasks;
using Orchard.Security.Permissions;
using Orchard.Security;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission)
        {
            return AuthorizeAsync(service, user, permission, null);
        }

        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission, object resource)
        {
            return service.AuthorizeAsync(user, resource, new PermissionRequirement(permission));
        }
    }
}
