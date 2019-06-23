using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization
{
    public class Permissions : IPermissionProvider
    {

        public static readonly Permission EditLocalizedContent = new Permission("EditLocalizedContent", "Edit content for others");
        public static readonly Permission EditOwnLocalizedContent = new Permission("EditOwnLocalizedContent", "Edit own content", new[] { EditLocalizedContent });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                EditLocalizedContent,
                EditOwnLocalizedContent,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {EditLocalizedContent, EditOwnLocalizedContent}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {EditLocalizedContent, EditOwnLocalizedContent}
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
                },
            };
        }

    }
}
