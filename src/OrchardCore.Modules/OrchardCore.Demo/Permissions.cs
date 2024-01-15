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
        public static readonly Permission DemoAPIAccess = new("DemoAPIAccess", "Access to Demo API ");
        public static readonly Permission ManageOwnUserProfile = new("ManageOwnUserProfile", "Manage own user profile", new Permission[] { CommonPermissions.ManageUsers });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { DemoAPIAccess, ManageOwnUserProfile }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Authenticated,
                    Permissions = new[] { DemoAPIAccess },
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Editor,
                    Permissions = new[] { ManageOwnUserProfile },
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Moderator,
                    Permissions = new[] { ManageOwnUserProfile },
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Contributor,
                    Permissions = new[] { ManageOwnUserProfile },
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Author,
                    Permissions = new[] { ManageOwnUserProfile },
                },
            };
        }
    }
}
