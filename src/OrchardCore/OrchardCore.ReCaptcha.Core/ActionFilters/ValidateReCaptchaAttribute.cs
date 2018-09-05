using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.Core.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Core.Configuration;
using OrchardCore.ReCaptcha.Core.Services;

namespace OrchardCore.ReCaptcha.Core.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        private readonly ReCaptchaMode _mode;


        public ValidateReCaptchaAttribute(ReCaptchaMode mode = ReCaptchaMode.AlwaysShow)
        {
            _mode = mode;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var recaptchaService = context.HttpContext.RequestServices.GetService<IReCaptchaService>();
            var isValidCaptcha = false;
            var reCaptchaResponse = context.HttpContext.Request?.Form?["g-recaptcha-response"].ToString();

            if (!String.IsNullOrWhiteSpace(reCaptchaResponse))
                isValidCaptcha = await recaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse);

            var isConvicted = false;

            switch (_mode)
            {
                case ReCaptchaMode.PreventAbuse:
                    isConvicted = await recaptchaService.IsConvictedAsync();
                    break;
                case ReCaptchaMode.AlwaysShow:
                    isConvicted = true;
                    break;

            }

            if (isConvicted && !isValidCaptcha)
                context.ModelState.AddModelError("ReCaptcha", "Failed to validate captcha");

            await next();

            if (context.ModelState.IsValid)
            {
                await recaptchaService.MarkAsInnocentAsync();
            }
            else
            {
                await recaptchaService.FlagAsSuspectAsync();
            }
        }
    }
}
