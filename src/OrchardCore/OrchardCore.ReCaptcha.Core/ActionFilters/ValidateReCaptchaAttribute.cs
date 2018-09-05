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
            //var abuseDetection = context.HttpContext.RequestServices.GetServices<IDetectAbuse>().ToList();
            //var logger = context.HttpContext.RequestServices.GetService<ILogger<ValidateReCaptchaAttribute>>();

            //// make sure the module is enabled and correctly configured
            //var moduleIsEnabled = settings?.IsValid() ?? false;

            //if (moduleIsEnabled)
            //{
            //    switch (_mode)
            //    {
            //        case ReCaptchaMode.AlwaysShow:
            //            await ValidateCaptchaAsync(context);
            //            break;
            //        case ReCaptchaMode.PreventAbuse:
            //            // If we suspected abuse, a captcha should be present
            //            if(abuseDetection.Invoke(ab => ab.DetectAbuse(context.HttpContext), logger).Any(a => a.SuspectAbuse))
            //                await ValidateCaptchaAsync(context);
            //            break;
            //    }
            //}

            var recaptchaService = context.HttpContext.RequestServices.GetService<IReCaptchaService>();
            var isValidCaptcha = false;
            var reCaptchaResponse = context.HttpContext.Request?.Form?["g-recaptcha-response"].ToString();

            if (!String.IsNullOrWhiteSpace(reCaptchaResponse))
                isValidCaptcha = await recaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse);

            var isConvicted = await recaptchaService.IsConvictedAsync();

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

        //private async Task ValidateCaptchaAsync(FilterContext context)
        //{
        //    var service = context.HttpContext.RequestServices.GetService<IReCaptchaService>();

        //    var reCaptchaResponse = context.HttpContext.Request?.Form?["g-recaptcha-response"].ToString();

        //    var isValid = await service.VerifyCaptchaResponseAsync(reCaptchaResponse);

        //    if (!isValid)
        //        context.ModelState.AddModelError("ReCaptcha", "Failed to validate captcha");
        //}
    }
}
