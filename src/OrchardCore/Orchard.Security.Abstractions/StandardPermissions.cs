using Orchard.Security.Permissions;

namespace Orchard.Security
{
    public class StandardPermissions
    {
        public static readonly Permission SiteOwner = new Permission("SiteOwner", "Site Owners Permission");
    }
}
