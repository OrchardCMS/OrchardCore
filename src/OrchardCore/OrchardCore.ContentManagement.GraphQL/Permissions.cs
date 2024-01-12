using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ApiViewContent = new("ApiViewContent", "Access view content endpoints");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ApiViewContent,
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ApiViewContent }
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Editor
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Moderator
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Author
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Contributor
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Authenticated
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Anonymous
                },
            };
        }
    }
}
