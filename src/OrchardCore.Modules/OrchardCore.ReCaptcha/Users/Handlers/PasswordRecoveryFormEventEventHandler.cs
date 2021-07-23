using System;
using System.Threading.Tasks;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class PasswordRecoveryFormEventEventHandler : IPasswordRecoveryFormEvents
    {
        private readonly ReCaptchaService _recaptchaService;

        public PasswordRecoveryFormEventEventHandler(ReCaptchaService recaptchaService)
        {
            _recaptchaService = recaptchaService;
        }

        public Task RecoveringPasswordAsync(Action<string, string> reportError)
        {
            return _recaptchaService.ValidateCaptchaAsync(reportError);
        }

        public Task PasswordResetAsync(PasswordRecoveryContext context)
        {
            return Task.CompletedTask;
        }

        public Task ResettingPasswordAsync(Action<string, string> reportError)
        {
            return _recaptchaService.ValidateCaptchaAsync(reportError);
        }

        public Task PasswordRecoveredAsync(PasswordRecoveryContext context)
        {
            return Task.CompletedTask;
        }
    }
}
