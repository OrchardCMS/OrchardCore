using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Twitter;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTwitterAuthentication
        = new Permission(nameof(ManageTwitterAuthentication), "Manage Twitter Authentication settings");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[] { ManageTwitterAuthentication }.AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        yield return new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new[]
            {
                ManageTwitterAuthentication
            }
        };
    }
}
