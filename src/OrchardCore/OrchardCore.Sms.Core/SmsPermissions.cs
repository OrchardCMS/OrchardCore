using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms;

public static class SmsPermissions
{
    public static readonly Permission ManageSmsSettings = new("ManageSmsSettings", LocalizationSource.Create("Manage SMS Settings", typeof(SmsPermissions)));
}
