using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.WebHooks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageWebHooks = new Permission("ManageWebHooks", "Manage webhooks");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageWebHooks };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageWebHooks }
                }
            };
        }
    }
}