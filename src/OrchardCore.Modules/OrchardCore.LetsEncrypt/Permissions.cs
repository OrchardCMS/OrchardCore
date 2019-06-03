using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.LetsEncrypt
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageLetsEncryptSettings = new Permission("ManageLetsEncryptSettings", "Manage Let's Encrypt Settings");
        public static readonly Permission ManageLetsEncryptAzureAuthSettings = new Permission("ManageLetsEncryptAzureAuthSettings", "Manage Let's Encrypt Azure Auth Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageLetsEncryptSettings,
                ManageLetsEncryptAzureAuthSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageLetsEncryptSettings, ManageLetsEncryptAzureAuthSettings }
                },
            };
        }
    }
}