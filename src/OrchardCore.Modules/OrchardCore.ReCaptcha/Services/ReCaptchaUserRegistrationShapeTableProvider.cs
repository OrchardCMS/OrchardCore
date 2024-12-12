using OrchardCore.ReCaptcha.Users.Handlers;

namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaUserRegistrationShapeTableProvider : ReCaptchaShapeTableProvider
{
    public ReCaptchaUserRegistrationShapeTableProvider()
        : base("RegisterUserForm_Edit", RegistrationFormEventHandler.UserRegistrationRobotTag)
    {
    }
}
