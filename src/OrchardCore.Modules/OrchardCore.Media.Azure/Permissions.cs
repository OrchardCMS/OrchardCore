using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ViewAzureMediaOptions = new(nameof(ViewAzureMediaOptions), "View Azure Media Options");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ViewAzureMediaOptions }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ViewAzureMediaOptions },
                },
            };
        }
    }
}
