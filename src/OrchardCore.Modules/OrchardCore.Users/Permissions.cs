using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Managing Users");
        public static readonly Permission EditOwnUserInformation = new Permission("EditOwnUserInformation", "Edit own user", new Permission[] { ManageUsers });
        // public static readonly Permission EditOwnUserPassword
        // dynamic for custom users, by code for own section.
        // public static readonly Permission EditOwnUserSection

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
            // TODO this might need a migration, as the default sterotypes are only created / set when the feature is initially enabled.
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
