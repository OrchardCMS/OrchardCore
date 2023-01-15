using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
        public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
        public static readonly Permission ManageOwnUserInformation = new("ManageOwnUserInformation", "Edit own user information", new Permission[] { CommonPermissions.EditUsers });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(new List<Permission>
            {
                ManageUsers,
                ViewUsers,
                ManageOwnUserInformation,
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
                    Permissions = new[] {
                        ManageUsers,
                        ViewUsers,
                        ManageOwnUserInformation,
                        CommonPermissions.ListUsers,
                        CommonPermissions.EditUsers,
                        CommonPermissions.DeleteUsers,
                    }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { ManageOwnUserInformation }
                }
            };
        }
    }
}
