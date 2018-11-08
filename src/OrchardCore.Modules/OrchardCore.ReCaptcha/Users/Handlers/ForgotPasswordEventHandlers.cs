using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class ForgotPasswordEventHandlers : ReCaptchaEventHandler, IForgotPasswordEvents
    {
        public ForgotPasswordEventHandlers(ReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor) : base(recaptchaService, contextAccessor)
        {
        }

        public Task ForgettingPasswordAsync(Action<string, string> reportError)
        {
            return ValidateCaptchaAsync(reportError);
        }

        public Task ForgotPasswordAsync()
        {
            return Task.CompletedTask;
        }

        public Task PasswordResetAsync()
        {
            return Task.CompletedTask;
        }

        public Task ResettingPasswordAsync(Action<string, string> reportError)
        {
            return ValidateCaptchaAsync(reportError);
        }
    }
}
