using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms.Azure;

public static class AzureSmsPermissions
{
    public static readonly Permission ManageAzureSmsSettings = new("ManageAzureSmsSettings", "Manage Azure SMS Settings");
}
