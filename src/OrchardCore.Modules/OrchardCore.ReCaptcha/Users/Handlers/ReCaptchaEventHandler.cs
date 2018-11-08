using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Services;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public abstract class ReCaptchaEventHandler
    {
        protected ReCaptchaEventHandler(ReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor)
        {
            ReCaptchaService = recaptchaService;
            HttpContextAccessor = contextAccessor;
        }

        protected ReCaptchaService ReCaptchaService { get; }

        protected IHttpContextAccessor HttpContextAccessor { get; }

        public async Task ValidateCaptchaAsync(Action<string, string> reportError)
        {
            var reCaptchaResponse = HttpContextAccessor.HttpContext.Request?.Form?[Constants.ReCaptchaServerResponseHeaderName].ToString();

            var isValid = !String.IsNullOrEmpty(reCaptchaResponse) && await ReCaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse);

            if (!isValid)
                reportError("ReCaptcha", "Failed to validate captcha");
        }
    }
}
