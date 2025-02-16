using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class LoginFormEventEventHandler : ILoginFormEvent
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginFormEventEventHandler(ReCaptchaService reCaptchaService, IHttpContextAccessor httpContextAccessor)
    {
        _reCaptchaService = reCaptchaService;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task IsLockedOutAsync(IUser user)
        => Task.CompletedTask;

    public Task LoggedInAsync(IUser user)
    {
        _reCaptchaService.ThisIsAHuman();

        return Task.CompletedTask;
    }

    public Task LoggingInAsync(string userName, Action<string, string> reportError)
    {
        // This is only a temporary fix for https://github.com/OrchardCMS/OrchardCore/issues/17422, to be used in a patch release.
        if (_httpContextAccessor.HttpContext?.Items.ContainsKey("IsExternalLogin") == true)
        {
            return Task.CompletedTask;
        }

        if (_reCaptchaService.IsThisARobot())
        {
            return _reCaptchaService.ValidateCaptchaAsync(reportError);
        }

        return Task.CompletedTask;
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
