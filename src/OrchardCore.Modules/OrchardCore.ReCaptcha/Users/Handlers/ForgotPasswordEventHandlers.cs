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
    public class ForgotPasswordEventHandlers : ReCaptchaEventHandler, IForgotPasswordEvents
    {
        public ForgotPasswordEventHandlers(IReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor) : base(recaptchaService, contextAccessor)
        {
        }

        public async Task ForgettingPasswordAsync(Action<string, string> reportError)
        {
            await ValidateCaptchaAsync(reportError);
        }

        public Task ForgotPasswordAsync()
        {
            return Task.CompletedTask;
        }

        public Task PasswordResetAsync()
        {
            return Task.CompletedTask;
        }

        public async Task ResettingPasswordAsync(Action<string, string> reportError)
        {
            await ValidateCaptchaAsync(reportError);
        }
    }
}
