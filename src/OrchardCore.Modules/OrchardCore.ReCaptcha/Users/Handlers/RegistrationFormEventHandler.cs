using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers;

public class RegistrationFormEventHandler : IRegistrationFormEvents
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegistrationFormEventHandler(ReCaptchaService reCaptchaService, IHttpContextAccessor httpContextAccessor)
    {
        _reCaptchaService = reCaptchaService;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task RegisteredAsync(IUser user)
    {
        return Task.CompletedTask;
    }

    public Task RegistrationValidationAsync(Action<string, string> reportError)
    {
        // This is only a temporary fix for https://github.com/OrchardCMS/OrchardCore/issues/17422, to be used in a patch release.
        if (_httpContextAccessor.HttpContext?.Items.ContainsKey("IsExternalLogin") == true)
        {
            return Task.CompletedTask;
        }

        return _reCaptchaService.ValidateCaptchaAsync(reportError);
    }
}
