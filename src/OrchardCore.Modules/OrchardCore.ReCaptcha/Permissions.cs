using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageReCaptchaSettings =
            new(
                "ManageReCaptchaSettings",
                "Manage ReCaptcha Settings"
            );

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageReCaptchaSettings,
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
                    Permissions = new[] { ManageReCaptchaSettings },
                }
            };
        }
    }
}
