using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization
{
    public class Permissions : IPermissionProvider
    {

        public static readonly Permission LocalizeContent = new Permission("LocalizeContent", "Localize content for others");
        public static readonly Permission LocalizeOwnContent = new Permission("LocalizeOwnContent", "Localize own content", new[] { LocalizeContent });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                LocalizeContent,
                LocalizeOwnContent,
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
                    Permissions = new[] { LocalizeContent, LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { LocalizeContent, LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = "Moderator"
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = new[] { LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = new[] { LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = "Authenticated"
                },
                new PermissionStereotype
                {
                    Name = "Anonymous"
                },
            };
        }
    }
}
