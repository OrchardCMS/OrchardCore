using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Core;

public static class SecurityConstants
{
    public static class Features
    {
        public const string Area = "OrchardCore.Security";

        public const string Credentials = "OrchardCore.Security.Credentials";
    }

    public static class Permissions
    {
        public static readonly Permission ManageSecurityHeadersSettings = new("ManageSecurityHeadersSettings", "Manage Security Headers Settings");

        public static readonly Permission ManageCredentials = new Permission("ManageCredentials", "Manage Credentials");
    }
}
