namespace OrchardCore.Users;

internal static class RateLimitRouteNames
{
    internal const string LinkExternalLogin = nameof(Controllers.ExternalAuthenticationsController.LinkExternalLogin);

    internal const string Login = "Login";

    internal const string LoginWithRecoveryCode = nameof(Controllers.TwoFactorAuthenticationController.LoginWithRecoveryCode);

    internal const string LoginWithTwoFactorAuthentication = nameof(Controllers.TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication);

    internal const string Register = nameof(Controllers.RegistrationController.Register);

    internal const string RegisterExternalLogin = nameof(Controllers.ExternalAuthenticationsController.RegisterExternalLogin);

    internal const string ResetPassword = nameof(Controllers.ResetPasswordController.ResetPassword);

    internal const string ForgotPassword = nameof(Controllers.ResetPasswordController.ForgotPassword);
}
