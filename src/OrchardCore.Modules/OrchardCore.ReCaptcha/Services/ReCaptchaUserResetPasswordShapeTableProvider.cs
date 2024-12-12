namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaUserResetPasswordShapeTableProvider : ReCaptchaShapeTableProvider
{
    public ReCaptchaUserResetPasswordShapeTableProvider()
        : base("ResetPasswordForm_Edit", "ForgotPasswordForm_Edit")
    {
    }
}
