using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ApiViewContent = new Permission("ApiViewContent", "Access view content endpoints");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ApiViewContent
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ApiViewContent}
                },
                new PermissionStereotype {
                    Name = "Editor"
                },
                new PermissionStereotype {
                    Name = "Moderator"
                },
                new PermissionStereotype {
                    Name = "Author"
                },
                new PermissionStereotype {
                    Name = "Contributor"
                },
                new PermissionStereotype {
                    Name = "Authenticated"
                },
                new PermissionStereotype {
                    Name = "Anonymous"
                }
            };
        }

    }
}