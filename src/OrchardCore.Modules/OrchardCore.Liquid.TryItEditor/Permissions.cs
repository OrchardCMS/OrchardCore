using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Liquid.TryItEditor
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission UseTryItEditor
            = new Permission(nameof(UseTryItEditor), "Open Liquid TryIt Editor.");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { UseTryItEditor }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    UseTryItEditor
                }
            };
        }
    }
}
