using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public class SeoPermissionProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[] { SeoConstants.ManageSeoSettings }.AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    SeoConstants.ManageSeoSettings
                }
            }
        };
    }
}
