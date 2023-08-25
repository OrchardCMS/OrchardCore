using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms;

public class SmsPermissions
{
    public static readonly Permission ManageSmsSettings = new("ManageSmsSettings", "Manage SMS Settings");
}
