using OrchardCore.ReCaptcha.Users.Handlers;

namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaUserResetPasswordShapeTableProvider : ReCaptchaShapeTableProvider
{
    public ReCaptchaUserResetPasswordShapeTableProvider()
        : base("ResetPasswordForm_Edit", PasswordRecoveryFormEventEventHandler.UserResetPasswordRobotTag)
    {
    }
}

internal sealed class ReCaptchaUserForgotPasswordShapeTableProvider : ReCaptchaShapeTableProvider
{
    public ReCaptchaUserForgotPasswordShapeTableProvider()
        : base("ForgotPasswordForm_Edit", PasswordRecoveryFormEventEventHandler.UserRecoverPasswordRobotTag)
    {
    }
}
