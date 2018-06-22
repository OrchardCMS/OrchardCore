using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Forms
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCaptchaSettings = new Permission("ManageCaptchaSettings", "Manage Captcha Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageCaptchaSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCaptchaSettings }
                }
            };
        }
    }
}