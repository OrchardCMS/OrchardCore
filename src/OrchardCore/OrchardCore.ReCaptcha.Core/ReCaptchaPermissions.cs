using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha;

public sealed class ReCaptchaPermissions
{
    public static readonly Permission ManageReCaptchaSettings = new("ManageReCaptchaSettings", "Manage ReCaptcha Settings");
}
