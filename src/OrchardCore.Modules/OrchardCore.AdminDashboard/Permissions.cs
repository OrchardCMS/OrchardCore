using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminDashboard = new Permission("ManageAdminDashboard", "Manage the Admin Dashboard");
        public static readonly Permission AccessAdminDashboard = new Permission("AccessAdminDashboard", "Access the Admin Dashboard", new[] { ManageAdminDashboard });

        public Permissions()
        {
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                AccessAdminDashboard,
                ManageAdminDashboard
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = BuiltInRole.Administrator,
                    Permissions = new[] { AccessAdminDashboard, ManageAdminDashboard }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Editor,
                    Permissions = new[] { AccessAdminDashboard }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Moderator,
                    Permissions = new[] { AccessAdminDashboard }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Author,
                    Permissions = new[] { AccessAdminDashboard }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Contributor,
                    Permissions = new[] { AccessAdminDashboard }
                }
            };
        }
    }
}
