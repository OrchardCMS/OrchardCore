using Microsoft.AspNetCore.Identity;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class LoginFormEventEventHandler : ILoginFormEvent
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly SignInManager<IUser> _signInManager;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService, SignInManager<IUser> signInManager)
    {
        _reCaptchaService = reCaptchaService;
        _signInManager = signInManager;
    }

    public Task IsLockedOutAsync(IUser user)
        => Task.CompletedTask;

    public Task LoggedInAsync(IUser user)
    {
        _reCaptchaService.ThisIsAHuman();

        return Task.CompletedTask;
    }

    public async Task LoggingInAsync(string userName, Action<string, string> reportError)
    {
        // When logging in via an external provider, authentication security is already handled by the provider.
        // Therefore, using a CAPTCHA is unnecessary and impractical, as users wouldn't be able to complete it anyway.
        if (!_reCaptchaService.IsThisARobot() || await _signInManager.GetExternalLoginInfoAsync() != null)
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }

    public Task LoggingInFailedAsync(string userName)
    {
        _reCaptchaService.MaybeThisIsARobot();

        return Task.CompletedTask;
    }

    public Task LoggingInFailedAsync(IUser user)
    {
        _reCaptchaService.MaybeThisIsARobot();

        return Task.CompletedTask;
    }
}
