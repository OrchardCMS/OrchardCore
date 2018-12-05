using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageMicrosoftAuthentication
            = new Permission(nameof(ManageMicrosoftAuthentication), "Manage Microsoft Authentication settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageMicrosoftAuthentication;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageMicrosoftAuthentication
                }
            };
        }
    }
}
