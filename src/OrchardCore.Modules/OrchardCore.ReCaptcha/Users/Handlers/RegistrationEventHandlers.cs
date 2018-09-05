using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using OrchardCore.ReCaptcha.Core.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class RegistrationEventHandlers : ReCaptchaEventHandler, IRegistrationEvents
    {
        public RegistrationEventHandlers(IReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor) : base(recaptchaService, contextAccessor)
        {

        }

        public Task RegisteredAsync()
        {
            return Task.CompletedTask;
        }

        public async Task RegisteringAsync(Action<string, string> reportError)
        {
            await ValidateCaptchaAsync(reportError);
        }
    }
}
