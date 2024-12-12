using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class LoginFormEventEventHandler : LoginFormEventBase
{
    public const string UserLoginRobotTag = "user-login";

    private readonly ReCaptchaService _reCaptchaService;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public override async Task LoggedInAsync(IUser user)
        => await _reCaptchaService.ThisIsAHumanAsync(UserLoginRobotTag);

    public override async Task LoggingInAsync(string userName, Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync(UserLoginRobotTag))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public override async Task LoggingInFailedAsync(string userName)
        => await _reCaptchaService.MaybeThisIsARobot(UserLoginRobotTag);

    public override async Task LoggingInFailedAsync(IUser user)
        => await _reCaptchaService.MaybeThisIsARobot(UserLoginRobotTag);
}
