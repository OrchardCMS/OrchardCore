using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
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

        public Task RegisteredAsync()
        {
            return Task.CompletedTask;
        }

        public Task RegistrationValidationAsync(Action<string, string> reportError)
        {
            return _reCaptchaService.ValidateCaptchaAsync(reportError);
        }
    }
}
