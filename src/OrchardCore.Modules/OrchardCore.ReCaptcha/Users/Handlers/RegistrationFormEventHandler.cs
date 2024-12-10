using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class RegistrationFormEventHandler : RegistrationFormEventsBase
{
    private readonly ReCaptchaService _reCaptchaService;

    public RegistrationFormEventHandler(ReCaptchaService recaptchaService)
    {
        _reCaptchaService = recaptchaService;
    }

    public override Task RegistrationValidationAsync(Action<string, string> reportError)
        => _reCaptchaService.ValidateCaptchaAsync(reportError);
}
