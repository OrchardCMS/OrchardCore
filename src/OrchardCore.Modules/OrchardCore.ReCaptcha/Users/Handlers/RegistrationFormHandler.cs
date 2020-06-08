using System;
using System.Threading.Tasks;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class RegistrationFormEventHandler : IRegistrationFormEvents
    {
        private readonly ReCaptchaService _reCaptchaService;

        public RegistrationFormEventHandler(ReCaptchaService recaptchaService)
        {
            _reCaptchaService = recaptchaService;
        }

        public Task RegisteredAsync(IUser user)
        {
            return Task.CompletedTask;
        }

        public Task RegistrationValidationAsync(Action<string, string> reportError)
        {
            return _reCaptchaService.ValidateCaptchaAsync(reportError);
        }
    }
}
