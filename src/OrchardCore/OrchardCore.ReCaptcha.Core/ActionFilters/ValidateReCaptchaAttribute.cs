using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ReCaptcha.Services;

namespace OrchardCore.ReCaptcha.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ValidateReCaptchaAttribute : ActionFilterAttribute
{
    private readonly ReCaptchaMode _mode;
    private readonly string _tag;

    public ValidateReCaptchaAttribute(ReCaptchaMode mode = ReCaptchaMode.AlwaysShow, string tag = "")
    {
        _mode = mode;
        _tag = tag ?? string.Empty;
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var reCaptchaService = context.HttpContext.RequestServices.GetService<ReCaptchaService>();
        var S = context.HttpContext.RequestServices.GetService<IStringLocalizer<ReCaptchaService>>();
        var isValidCaptcha = false;
        var reCaptchaResponse = context.HttpContext.Request?.Form?[Constants.ReCaptchaServerResponseHeaderName].ToString();

        if (!string.IsNullOrWhiteSpace(reCaptchaResponse))
        {
            isValidCaptcha = await reCaptchaService.VerifyCaptchaResponseAsync(reCaptchaResponse);
        }

        var isRobot = true;

        if (_mode == ReCaptchaMode.PreventRobots)
        {
            isRobot = await reCaptchaService.IsThisARobotAsync(_tag);
        }

        if (isRobot && !isValidCaptcha)
        {
            context.ModelState.AddModelError("ReCaptcha", S["Failed to validate captcha"]);
        }

        await next();

        if (context.ModelState.IsValid)
        {
            await reCaptchaService.ThisIsAHumanAsync(_tag);
        }
        else
        {
            await reCaptchaService.MaybeThisIsARobot(_tag);
        }

        return;
    }
}
