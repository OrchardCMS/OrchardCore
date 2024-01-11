using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminDashboard = new("ManageAdminDashboard", "Manage the Admin Dashboard");
        public static readonly Permission AccessAdminDashboard = new("AccessAdminDashboard", "Access the Admin Dashboard", new[] { ManageAdminDashboard });

        public Permissions()
        {
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                AccessAdminDashboard,
                ManageAdminDashboard,
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { AccessAdminDashboard, ManageAdminDashboard },
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { AccessAdminDashboard },
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                    Permissions = new[] { AccessAdminDashboard },
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = new[] { AccessAdminDashboard },
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = new[] { AccessAdminDashboard },
                }
            };
        }
    }
}
