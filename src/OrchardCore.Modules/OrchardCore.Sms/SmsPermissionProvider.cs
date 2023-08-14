using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms;

public class SmsPermissionProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[] { SmsPermissions.ManageSmsSettings }.AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        yield return new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new[]
            {
                SmsPermissions.ManageSmsSettings
            }
        };
    }
}
