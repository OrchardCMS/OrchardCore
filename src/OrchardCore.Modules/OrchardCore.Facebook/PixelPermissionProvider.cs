using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public class PixelPermissionProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[] { FacebookConstants.ManageFacebookPixelPermission }.AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        yield return new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new[]
            {
                FacebookConstants.ManageFacebookPixelPermission
            }
        };
    }
}
