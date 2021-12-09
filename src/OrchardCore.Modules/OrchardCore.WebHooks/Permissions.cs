using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.WebHooks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageWebHooks = new Permission("ManageWebHooks", "Manage webhooks");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageWebHooks }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageWebHooks }
                }
            };
        }
    }
}
