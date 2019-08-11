using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageReCaptchaSettings = 
            new Permission(
                "ManageReCaptchaSettings", 
                "Manage ReCaptcha Settings"
                );

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageReCaptchaSettings,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] 
            {
                new PermissionStereotype 
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageReCaptchaSettings }
                },
            };
        }

    }
}
