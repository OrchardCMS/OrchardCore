using System;
using System.Threading.Tasks;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class LoginFormEventEventHandler : ILoginFormEvent
    {
        private readonly ReCaptchaService _reCaptchaService;

        public LoginFormEventEventHandler(ReCaptchaService reCaptchaService)
        {
            _reCaptchaService = reCaptchaService;
        }

        public Task LoggedInAsync(string userName)
        {
            _reCaptchaService.ThisIsAHuman();
            return Task.CompletedTask;
        }

        public async Task LoggingInAsync(string userName, Action<string, string> reportError)
        {
            if (_reCaptchaService.IsThisARobot())
            {
                await _reCaptchaService.ValidateCaptchaAsync(reportError);
            }
        }

        public Task LoggingInFailedAsync(string userName)
        {
            _reCaptchaService.MaybeThisIsARobot();
            return Task.CompletedTask;
        }
    }
}
