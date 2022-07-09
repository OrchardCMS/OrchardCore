using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Demo
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission DemoAPIAccess = new Permission("DemoAPIAccess", "Access to Demo API ");
        public static readonly Permission ManageOwnUserProfile = new Permission("ManageOwnUserProfile", "Manage own user profile", new Permission[] { CommonPermissions.ManageUsers });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { DemoAPIAccess, ManageOwnUserProfile }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype {
                    Name = BuiltInRole.Authenticated,
                    Permissions = new[] { DemoAPIAccess }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Editor,
                    Permissions = new[] { ManageOwnUserProfile }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Moderator,
                    Permissions = new[] { ManageOwnUserProfile }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Contributor,
                    Permissions = new[] { ManageOwnUserProfile }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Author,
                    Permissions = new[] { ManageOwnUserProfile }
                }
            };
        }
    }
}
