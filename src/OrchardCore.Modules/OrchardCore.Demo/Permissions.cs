using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Demo
{
    public class Permissions : IPermissionProvider
    {
        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        public static readonly Permission DemoAPIAccess = new Permission("DemoAPIAccess", "Access to Demo API ");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { DemoAPIAccess }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] { DemoAPIAccess }
                }
            };
        }
    }
}
