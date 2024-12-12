using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class PasswordRecoveryFormEventEventHandler : IPasswordRecoveryFormEvents
{
    public const string UserRecoverPasswordRobotTag = "user-recover-password";
    public const string UserResetPasswordRobotTag = "user-reset-password";

    private readonly ReCaptchaService _reCaptchaService;

    public PasswordRecoveryFormEventEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public async Task RecoveringPasswordAsync(Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync(UserRecoverPasswordRobotTag))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public async Task PasswordRecoveredAsync(PasswordRecoveryContext context)
        => await _reCaptchaService.ThisIsAHumanAsync(UserRecoverPasswordRobotTag);

    public async Task ResettingPasswordAsync(Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync(UserResetPasswordRobotTag))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public async Task PasswordResetAsync(PasswordRecoveryContext context)
        => await _reCaptchaService.ThisIsAHumanAsync(UserResetPasswordRobotTag);
}
