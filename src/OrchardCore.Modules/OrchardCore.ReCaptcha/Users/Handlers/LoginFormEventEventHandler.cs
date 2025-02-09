using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public sealed class LoginFormEventEventHandler : LoginFormEventBase
{
    private readonly ReCaptchaService _reCaptchaService;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public override Task LoggingInAsync(string userName, Action<string, string> reportError)
        => _reCaptchaService.ValidateCaptchaAsync(reportError);
}
