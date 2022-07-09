using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission LocalizeContent = new Permission("LocalizeContent", "Localize content for others");
        public static readonly Permission LocalizeOwnContent = new Permission("LocalizeOwnContent", "Localize own content", new[] { LocalizeContent });
        public static readonly Permission ManageContentCulturePicker = new Permission("ManageContentCulturePicker", "Manage ContentCulturePicker settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                LocalizeContent,
                LocalizeOwnContent,
                ManageContentCulturePicker
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
                    Permissions = new[] { LocalizeContent, LocalizeOwnContent, ManageContentCulturePicker }
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Editor,
                    Permissions = new[] { LocalizeContent, LocalizeOwnContent, ManageContentCulturePicker }
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Moderator
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Author,
                    Permissions = new[] { LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Contributor,
                    Permissions = new[] { LocalizeOwnContent }
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Authenticated
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Anonymous
                },
            };
        }
    }
}
