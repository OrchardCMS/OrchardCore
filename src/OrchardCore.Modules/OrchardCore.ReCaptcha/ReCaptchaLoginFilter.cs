using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Entities;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha
{
    public class ReCaptchaLoginFilter : IAsyncResultFilter
    {
        private ILayoutAccessor _layoutAccessor;
        private ISiteService _siteService;
        private ReCaptchaService _reCaptchaService;
        private IShapeFactory _shapeFactory;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!(context.Result is ViewResult || context.Result is PageResult)
                || !String.Equals("OrchardCore.Users", Convert.ToString(context.RouteData.Values["area"]), StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // Resolve scoped services lazy if we got this far.
            _siteService ??= context.HttpContext.RequestServices.GetRequiredService<ISiteService>();
            var settings = (await _siteService.GetSiteSettingsAsync()).As<ReCaptchaSettings>();

            if (!settings.IsValid())
            {
                await next();
                return;
            }

            // Resolve scoped services lazy if we got this far.
            _layoutAccessor ??= context.HttpContext.RequestServices.GetRequiredService<ILayoutAccessor>();
            _reCaptchaService ??= context.HttpContext.RequestServices.GetRequiredService<ReCaptchaService>();
            _shapeFactory ??= context.HttpContext.RequestServices.GetRequiredService<IShapeFactory>();

            dynamic layout = await _layoutAccessor.GetLayoutAsync();

            var afterLoginZone = layout.Zones["AfterLogin"];

            if (_reCaptchaService.IsThisARobot())
            {
                afterLoginZone.Add(await _shapeFactory.New.ReCaptcha());
            }

            var afterForgotPassword = layout.Zones["AfterForgotPassword"];
            afterForgotPassword.Add(await _shapeFactory.New.ReCaptcha());

            var afterRegister = layout.Zones["AfterRegister"];
            afterRegister.Add(await _shapeFactory.New.ReCaptcha());

            var afterResetPassword = layout.Zones["AfterResetPassword"];
            afterResetPassword.Add(await _shapeFactory.New.ReCaptcha());

            await next();
        }
    }
}
