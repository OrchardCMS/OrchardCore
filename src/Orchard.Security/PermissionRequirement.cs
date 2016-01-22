using Microsoft.AspNet.Authorization;
using Orchard.Security.Permissions;

namespace Orchard.Security
{
    public class PermissionRequirement : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
    {
        private readonly Permission _permission;

        public PermissionRequirement(Permission permission)
        {
            _permission = permission;
        }

        protected override void Handle(AuthorizationContext context, PermissionRequirement resource)
        {
            if(context.User == null)
            {
                context.Succeed(resource);
            }

            if (context.User.IsInRole("Administrator"))
            {
                context.Succeed(resource);
                return;
            }

            if (context.User.HasClaim(Permission.ClaimType, _permission.Name))
            {
                context.Succeed(resource);
                return;
            }
        }
    }
}
