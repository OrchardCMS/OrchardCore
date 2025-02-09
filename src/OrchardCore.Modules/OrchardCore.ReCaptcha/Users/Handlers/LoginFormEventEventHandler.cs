using Microsoft.AspNetCore.Identity;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public sealed class LoginFormEventEventHandler : LoginFormEventBase
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly UserManager<IUser> _userManager;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService, UserManager<IUser> userManager)
    {
        _reCaptchaService = reCaptchaService;
        _userManager = userManager;
    }

    public override async Task LoggingInAsync(string userName, Action<string, string> reportError)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            return;
        }

        // Checking if the user has any non-local logins. We shouldn't try to validate the captcha for users that log
        // in with e.g. Microsoft Entra ID.
        var logins = await _userManager.GetLoginsAsync(user);

        if (logins.Count > 0)
        {
            return;
        }

        await _reCaptchaService.ValidateCaptchaAsync(reportError);
    }
}
