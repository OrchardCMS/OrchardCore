using System.Security.Claims;
using System.Threading.Tasks;
using Orchard.Security.Permissions;

namespace Microsoft.AspNet.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission)
        {
            var policy = new AuthorizationPolicyBuilder().RequireClaim(Permission.ClaimType, permission.Name).Build();
            return service.AuthorizeAsync(user, policy);
        }
    }
}
