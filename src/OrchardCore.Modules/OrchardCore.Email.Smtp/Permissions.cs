using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSmtpEmailSettings = new("ManageSmtpEmailSettings", "Manage SMTP Email Settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageSmtpEmailSettings,
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
                    Permissions = new[] { ManageSmtpEmailSettings },
                },
            };
        }
    }
}
