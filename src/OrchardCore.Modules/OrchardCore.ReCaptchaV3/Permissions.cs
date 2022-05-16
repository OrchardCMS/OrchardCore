using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptchaV3
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageReCaptchaV3Settings =
            new Permission
            (
                "ManageReCaptchaV3Settings",
                "Manage ReCaptchaV3 Settings"
            );

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageReCaptchaV3Settings
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
                    Permissions = new[] { ManageReCaptchaV3Settings }
                }
            };
        }
    }
}
