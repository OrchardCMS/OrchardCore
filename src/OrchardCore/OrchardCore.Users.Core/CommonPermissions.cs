using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class CommonPermissions
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Managing Users");
    }
}
