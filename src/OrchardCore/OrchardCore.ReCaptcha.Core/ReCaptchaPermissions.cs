using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha;

public static class ReCaptchaPermissions
{
    public static readonly Permission ManageReCaptchaSettings = new("ManageReCaptchaSettings", new LocalizationSource("Manage ReCaptcha Settings", typeof(ReCaptchaPermissions)));
}
