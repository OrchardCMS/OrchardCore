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

        public ValidateReCaptchaAttribute() : this(ReCaptchaMode.AlwaysShow)
        {

        }

        public ValidateReCaptchaAttribute(ReCaptchaMode mode)
        {
            _mode = mode;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var settings = context.HttpContext.RequestServices.GetService<IOptions<ReCaptchaSettings>>().Value;
            var abuseDetection = context.HttpContext.RequestServices.GetServices<IDetectAbuse>().ToList();
            var logger = context.HttpContext.RequestServices.GetService<ILogger<ValidateReCaptchaAttribute>>();

            // make sure the module is enabled and correctly configured
            var moduleIsEnabled = settings?.IsValid() ?? false;

            if (moduleIsEnabled)
            {
                switch (_mode)
                {
                    case ReCaptchaMode.AlwaysShow:
                        await ValidateCaptchaAsync(settings.SecretKey, context);
                        break;
                    case ReCaptchaMode.PreventAbuse:
                        // If we suspected abuse, a captcha should be present
                        if(abuseDetection.Invoke(ab => ab.DetectAbuse(context.HttpContext), logger).Any(a => a.SuspectAbuse))
                            await ValidateCaptchaAsync(settings.SecretKey, context);
                        break;
                }
            }

            await next();

            if (moduleIsEnabled)
            {
                if (context.ModelState.IsValid)
                {
                    abuseDetection?.Invoke(ab => ab.ClearAbuseFlags(context.HttpContext), logger);
                }
                else
                {
                    abuseDetection?.Invoke(ab => ab.FlagPossibleAbuse(context.HttpContext), logger);
                }
            }
        }

        private async Task ValidateCaptchaAsync(string secretKey, FilterContext context)
        {
            var client = context.HttpContext.RequestServices.GetService<IReCaptchaClient>();

            var reCaptchaResponse = context.HttpContext.Request?.Form?["g-recaptcha-response"].ToString();

            var isValid = !String.IsNullOrEmpty(reCaptchaResponse);

            if (isValid)
                isValid = await client.VerifyAsync(reCaptchaResponse, secretKey);

            if (!isValid)
                context.ModelState.AddModelError("ReCaptcha", "Failed to validate captcha");
        }
    }
}
