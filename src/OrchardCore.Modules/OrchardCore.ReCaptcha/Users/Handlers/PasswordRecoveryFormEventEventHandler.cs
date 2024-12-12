using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class PasswordRecoveryFormEventEventHandler : IPasswordRecoveryFormEvents
{
    private readonly ReCaptchaService _reCaptchaService;

    public PasswordRecoveryFormEventEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public async Task RecoveringPasswordAsync(Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync("user-recover-password"))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public async Task PasswordRecoveredAsync(PasswordRecoveryContext context)
        => await _reCaptchaService.ThisIsAHumanAsync("user-recover-password");

    public async Task ResettingPasswordAsync(Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync("user-reset-password"))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public async Task PasswordResetAsync(PasswordRecoveryContext context)
        => await _reCaptchaService.ThisIsAHumanAsync("user-reset-password");
}
