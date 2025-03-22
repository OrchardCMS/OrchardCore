using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public static class EmailPermissions
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings");
}
