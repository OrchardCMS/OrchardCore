using System.Security.Claims;
using System.Threading.Tasks;
using Orchard.Security.Permissions;
using Orchard.Security;

namespace Microsoft.AspNet.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission)
        {
            return service.AuthorizeAsync(user, null, permission);
        }

        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission, object resource)
        {
            var requirement = new PermissionRequirement(permission);
            return service.AuthorizeAsync(user, resource, requirement);
        }
    }
}
