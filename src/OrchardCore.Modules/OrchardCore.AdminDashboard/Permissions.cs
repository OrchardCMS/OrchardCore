using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.AdminDashboard.Services;

namespace OrchardCore.AdminDashboard
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminDashboard = new Permission("ManageAdminDashboard", "Manage the Admin Dashboard");

        public Permissions()
        {
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminDashboard }
                }
            };
        }

        private IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageAdminDashboard
            };
        }
    }
}
