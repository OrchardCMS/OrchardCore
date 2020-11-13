using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Managing Users");
        public static readonly Permission EditOwnUserInformation = new Permission("EditOwnUserInformation", "Edit own user information", new Permission[] { ManageUsers });
        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageUsers,
                EditOwnUserInformation
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageUsers }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { EditOwnUserInformation }
                }
            };
        }
    }
}
