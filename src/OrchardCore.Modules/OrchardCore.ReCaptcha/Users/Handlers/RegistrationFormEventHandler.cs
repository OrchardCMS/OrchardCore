using Microsoft.AspNetCore.Identity;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public sealed class RegistrationFormEventHandler : RegistrationFormEventsBase
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly SignInManager<IUser> _signInManager;

    public RegistrationFormEventHandler(
        ReCaptchaService reCaptchaService,
        SignInManager<IUser> signInManager)
    {
        _reCaptchaService = reCaptchaService;
        _signInManager = signInManager;
    }

    public override async Task RegistrationValidationAsync(Action<string, string> reportError)
    {
        // When the login is using an external provider, then we offload authentication security to it already, so no
        // point in using a captcha (and currently users wouldn't be able to fill it out anyway).
        if (await _signInManager.GetExternalLoginInfoAsync() != null)
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }
}
