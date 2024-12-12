using OrchardCore.ReCaptcha.Users.Handlers;

namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaUserShapeTableProvider : ReCaptchaShapeTableProvider
{
    public ReCaptchaUserShapeTableProvider()
        : base("LoginForm_Edit", LoginFormEventEventHandler.UserLoginRobotTag)
    {
    }
}
