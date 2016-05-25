using System;
using Microsoft.AspNetCore.Authorization;
using Orchard.Security.Permissions;

namespace Orchard.Security
{
    public class PermissionRequirement : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
    {
        private readonly Permission _permission;

        public PermissionRequirement(Permission permission)
        {
            if(permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            _permission = permission;
        }

        protected override void Handle(AuthorizationContext context, PermissionRequirement resource)
        {
            if (context.User == null)
            {
                context.Fail();
                return;
            }
            if (context.User.Identity.IsAuthenticated)
            {
                // Authorize all users for now
                context.Succeed(resource);
                return;
            }
            else if (context.User.IsInRole("Administrator"))
            {
                context.Succeed(resource);
                return;
            }
            else if (context.User.HasClaim(Permission.ClaimType, _permission.Name))
            {
                context.Succeed(resource);
                return;
            }

            context.Fail();
        }
    }
}
