using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public sealed class EmailPermissions
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings");
}
