using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.Localization.ViewModels;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Localization.ViewComponents
{
    public class CulturePickerViewComponent: ViewComponent
    {
        private readonly IList<CultureInfo> _supportedUICultures;
        private readonly IResourceManager _resourceManager;

        public CulturePickerViewComponent(
            IOptions<RequestLocalizationOptions> localizationOptions,
            IResourceManager resourceManager)
        {
            _supportedUICultures = localizationOptions.Value.SupportedUICultures;
            _resourceManager = resourceManager;
        }

        public IViewComponentResult Invoke()
        {
            AppendLocalizationCookieScript();

            return View(new CulturePickerViewModel {
                SelectedCulture = ViewContext.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name,
                SupportedUICultures = _supportedUICultures
            });
        }

        private void AppendLocalizationCookieScript()
        {
            var localizationScript = new TagBuilder("script");
            localizationScript.InnerHtml.AppendHtml($@"function setLocalizationCookie(code) {{
        document.cookie = '{CookieRequestCultureProvider.DefaultCookieName}=c='+code+'|uic='+code;
        window.location.reload();
    }}");
            _resourceManager.RegisterFootScript(localizationScript);
        }
    }
}