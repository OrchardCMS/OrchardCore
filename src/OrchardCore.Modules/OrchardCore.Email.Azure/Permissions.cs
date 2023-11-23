using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email.Azure
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ViewAzureEmailOptions = new(nameof(ViewAzureEmailOptions), "View Azure Email Options");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ViewAzureEmailOptions,
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ViewAzureEmailOptions },
                },
            };
        }
    }
}
