using System;
using System.Threading.Tasks;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
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

        public Task IsLockedOutAsync(IUser user) => Task.CompletedTask;

        public Task LoggedInAsync(IUser user)
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

        public Task LoggingInFailedAsync(IUser user)
        {
            _reCaptchaService.MaybeThisIsARobot();
            return Task.CompletedTask;
        }
    }
}
