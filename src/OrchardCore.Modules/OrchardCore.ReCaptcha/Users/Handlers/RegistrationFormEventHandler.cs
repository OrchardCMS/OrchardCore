using Microsoft.AspNetCore.Identity;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class RegistrationFormEventHandler : IRegistrationFormEvents
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly SignInManager<IUser> _signInManager;

    public RegistrationFormEventHandler(ReCaptchaService reCaptchaService, SignInManager<IUser> signInManager)
    {
        _reCaptchaService = reCaptchaService;
        _signInManager = signInManager;
    }

    public Task RegisteredAsync(IUser user)
    {
        return Task.CompletedTask;
    }

    public async Task RegistrationValidationAsync(Action<string, string> reportError)
    {
        // When logging in via an external provider, authentication security is already handled by the provider.
        // Therefore, using a CAPTCHA is unnecessary and impractical, as users wouldn't be able to complete it anyway.
        if (await _signInManager.GetExternalLoginInfoAsync() != null)
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }
}
