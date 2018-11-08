using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class RegistrationEventHandlers : ReCaptchaEventHandler, IRegistrationEvents
    {
        public RegistrationEventHandlers(ReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor) : base(recaptchaService, contextAccessor)
        {

        }

        public Task RegisteredAsync()
        {
            return Task.CompletedTask;
        }

        public Task RegisteringAsync(Action<string, string> reportError)
        {
            return ValidateCaptchaAsync(reportError);
        }
    }
}
