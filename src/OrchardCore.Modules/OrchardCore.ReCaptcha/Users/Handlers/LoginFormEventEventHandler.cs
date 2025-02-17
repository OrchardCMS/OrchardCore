using Microsoft.AspNetCore.Identity;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public sealed class LoginFormEventEventHandler : LoginFormEventBase
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly SignInManager<IUser> _signInManager;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService, SignInManager<IUser> signInManager)
    {
        _reCaptchaService = reCaptchaService;
        _signInManager = signInManager;
    }

    public override async Task LoggingInAsync(string userName, Action<string, string> reportError)
    {
        if (await _signInManager.GetExternalLoginInfoAsync() == null)
        {
            await _reCaptchaService.ValidateCaptchaAsync(reportError);
        }
    }
}
