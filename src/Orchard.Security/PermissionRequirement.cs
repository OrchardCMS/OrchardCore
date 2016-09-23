using System;
using Microsoft.AspNetCore.Authorization;
using Orchard.Security.Permissions;

namespace Orchard.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(Permission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            Permission = permission;
        }

        public Permission Permission { get; set; }
    }
}


