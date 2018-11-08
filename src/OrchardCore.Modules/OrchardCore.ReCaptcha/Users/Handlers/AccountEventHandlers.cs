using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class AccountEventHandlers : ReCaptchaEventHandler, IAccountEvents
    {
        public AccountEventHandlers(ReCaptchaService reCaptchaService, IHttpContextAccessor httpContextAccessor) : base(reCaptchaService, httpContextAccessor)
        {

        }

        public Task LoggedInAsync()
        {
            ReCaptchaService.MarkAsInnocent();
            return Task.CompletedTask;
        }

        public async Task LoggingInAsync(Action<string, string> reportError)
        {
            if(ReCaptchaService.IsConvicted())
                await ValidateCaptchaAsync(reportError);
        }

        public Task LoggingInFailedAsync()
        {
            ReCaptchaService.FlagAsSuspect();
            return Task.CompletedTask;
        }
    }
}
