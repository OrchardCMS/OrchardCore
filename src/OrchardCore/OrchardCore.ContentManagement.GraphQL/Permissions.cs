using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ApiViewContent = new Permission("ApiViewContent", "Access view content endpoints");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ApiViewContent
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name =  BuiltInRole.Administrator,
                    Permissions = new[] { ApiViewContent }
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Editor
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Moderator
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Author
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Contributor
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Authenticated
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Anonymous
                }
            };
        }
    }
}
