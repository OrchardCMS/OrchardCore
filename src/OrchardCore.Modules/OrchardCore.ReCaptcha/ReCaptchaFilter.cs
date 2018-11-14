using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Settings;
using OrchardCore.Entities;

namespace OrchardCore.ReCaptcha
{
    public class ReCaptchaFilter : IActionFilter, IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ISiteService _siteService;
        private readonly ILogger<ReCaptchaFilter> _logger;
        private readonly ReCaptchaService _reCaptchaService;
        private readonly IShapeFactory _shapeFactory;

        public ReCaptchaFilter(
            ILayoutAccessor layoutAccessor,
            ISiteService siteService,
            ReCaptchaService reCaptchaService,
            IShapeFactory shapeFactory,
            ILogger<ReCaptchaFilter> logger)
        {
            _layoutAccessor = layoutAccessor;
            _siteService = siteService;
            _reCaptchaService = reCaptchaService;
            _shapeFactory = shapeFactory;
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        public async Task OnResultExecutionAsync(ResultExecutingContext filterContext, ResultExecutionDelegate next)
        {
            if (!(filterContext.Result is ViewResult))
            {
                await next();
                return;
            }

            var settings = (await _siteService.GetSiteSettingsAsync()).As<ReCaptchaSettings>();

            if (!settings.IsValid())
                await next();

            dynamic layout = await _layoutAccessor.GetLayoutAsync();

            if (settings.HardenLoginProcess)
            {
                var afterLoginZone = layout.Zones["AfterLogin"];
                if (_reCaptchaService.IsConvicted())
                    afterLoginZone.Add(await _shapeFactory.New.ReCaptcha());
            }

            if (settings.HardenForgotPasswordProcess)
            {
                var afterForgotPassword = layout.Zones["AfterForgotPassword"];
                afterForgotPassword.Add(await _shapeFactory.New.ReCaptcha());
            }

            if (settings.HardenRegisterProcess)
            {
                var afterRegister = layout.Zones["AfterRegister"];
                afterRegister.Add(await _shapeFactory.New.ReCaptcha());
            }

            await next();
        }
    }
}