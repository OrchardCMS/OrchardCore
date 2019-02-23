using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSitemaps = new Permission("ManageSitemaps", "Manage sitemaps");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageSitemaps };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSitemaps }
                }
            };
        }
    }
}