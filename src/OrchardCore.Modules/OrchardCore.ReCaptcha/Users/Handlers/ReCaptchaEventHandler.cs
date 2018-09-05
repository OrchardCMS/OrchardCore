using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ReCaptcha.Core.Services;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public abstract class ReCaptchaEventHandler
    {
        private readonly IReCaptchaService _recaptchaService;
        private readonly IHttpContextAccessor _contextAccessor;

        protected ReCaptchaEventHandler(IReCaptchaService recaptchaService, IHttpContextAccessor contextAccessor)
        {
            _recaptchaService = recaptchaService;
            _contextAccessor = contextAccessor;
        }

        protected IReCaptchaService ReCaptchaService => _recaptchaService;

        protected IHttpContextAccessor HttpContextAccessor => _contextAccessor;

        public async Task ValidateCaptchaAsync(Action<string, string> reportError)
        {
            var reCaptchaResponse = _contextAccessor.HttpContext.Request?.Form?["g-recaptcha-response"].ToString();

            var isValid = !String.IsNullOrEmpty(reCaptchaResponse);

            if (isValid)
                isValid = await _recaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse);

            if (!isValid)
                reportError("ReCaptcha", "Failed to validate captcha");
        }
    }
}
