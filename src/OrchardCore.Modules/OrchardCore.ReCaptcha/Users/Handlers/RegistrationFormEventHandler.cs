using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public sealed class RegistrationFormEventHandler : RegistrationFormEventsBase
{
    private readonly ReCaptchaService _reCaptchaService;

    public RegistrationFormEventHandler(ReCaptchaService reCaptchaService)
    {
        _reCaptchaService = reCaptchaService;
    }

    public override Task RegistrationValidationAsync(Action<string, string> reportError)
        => _reCaptchaService.ValidateCaptchaAsync(reportError);
}
