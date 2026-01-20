using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ReCaptcha.Services;

namespace OrchardCore.ReCaptcha.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ValidateReCaptchaAttribute : ActionFilterAttribute
{
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var reCaptchaService = context.HttpContext.RequestServices.GetRequiredService<ReCaptchaService>();
        var reCaptchaResponse = context.HttpContext.Request?.Form?[Constants.ReCaptchaServerResponseHeaderName].ToString();

        if (string.IsNullOrWhiteSpace(reCaptchaResponse) || !await reCaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse))
        {
            var S = context.HttpContext.RequestServices.GetService<IStringLocalizer<ReCaptchaService>>();

            context.ModelState.AddModelError("ReCaptcha", S["Failed to validate captcha"]);
        }

        await next();
    }
}
