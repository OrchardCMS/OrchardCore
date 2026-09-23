using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha;

public static class ReCaptchaPermissions
{
    public static readonly Permission ManageReCaptchaSettings = new("ManageReCaptchaSettings", LocalizedString.Create("Manage ReCaptcha Settings", typeof(ReCaptchaPermissions)));
}
