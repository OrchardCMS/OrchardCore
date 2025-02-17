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
        if (await _signInManager.GetExternalLoginInfoAsync() == null)
        {
            await _reCaptchaService.ValidateCaptchaAsync(reportError);
        }
    }
}
