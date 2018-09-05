using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Core.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class AccountEventHandlers : ReCaptchaEventHandler, IAccountEvents
    {
        public AccountEventHandlers(IReCaptchaService reCaptchaService, IHttpContextAccessor httpContextAccessor) : base(reCaptchaService, httpContextAccessor)
        {

        }

        public async Task LoggedInAsync()
        {
            await ReCaptchaService.MarkAsInnocentAsync();
        }

        public async Task LoggingInAsync(Action<string, string> reportError)
        {
            if(await ReCaptchaService.IsConvictedAsync())
                await ValidateCaptchaAsync(reportError);
        }

        public async Task LoggingInFailedAsync()
        {
            await ReCaptchaService.FlagAsSuspectAsync();
        }
    }
}
