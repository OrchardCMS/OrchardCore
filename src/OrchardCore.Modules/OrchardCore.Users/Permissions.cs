using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
        public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
        public static readonly Permission ManageOwnUserInformation = CommonPermissions.EditOwnUser;

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(new List<Permission>
            {
                CommonPermissions.ManageUsers,
                CommonPermissions.ViewUsers,
                CommonPermissions.EditOwnUser,
                CommonPermissions.ListUsers,
                CommonPermissions.EditUsers,
                CommonPermissions.DeleteUsers,
            });
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        CommonPermissions.ManageUsers,
                        CommonPermissions.ViewUsers,
                        CommonPermissions.EditOwnUser,
                        CommonPermissions.ListUsers,
                        CommonPermissions.EditUsers,
                        CommonPermissions.DeleteUsers,
                    }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { CommonPermissions.EditOwnUser },
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                    Permissions = new[] { CommonPermissions.EditOwnUser },
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = new[] { CommonPermissions.EditOwnUser },
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = new[] { CommonPermissions.EditOwnUser },
                }
            };
        }
    }
}
