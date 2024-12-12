using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class RegistrationFormEventHandler : RegistrationFormEventsBase
{
    public const string UserRegistrationRobotTag = "user-registration";

    private readonly ReCaptchaService _reCaptchaService;

    public RegistrationFormEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public override async Task RegistrationValidationAsync(Action<string, string> reportError)
    {
        if (!await _reCaptchaService.IsThisARobotAsync(UserRegistrationRobotTag))
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public override async Task RegisteringAsync(UserRegisteringContext context)
        => await _reCaptchaService.MaybeThisIsARobot(UserRegistrationRobotTag);

    public override async Task RegisteredAsync(IUser user)
        => await _reCaptchaService.ThisIsAHumanAsync(UserRegistrationRobotTag);
}
