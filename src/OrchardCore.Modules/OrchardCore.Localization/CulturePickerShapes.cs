using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Localization
{
    public class CulturePickerShapes : IShapeTableProvider
    {
        private IResourceManager _resourceManager;

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("CulturePicker")
                .OnDisplayed(context =>
                {
                    _resourceManager = context.ServiceProvider.GetRequiredService<IResourceManager>();
                    AppendLocalizationCookieScript();
                })
                .OnProcessing(async context =>
                {
                    var culturePicker = context.Shape;
                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var localizationService = httpContextAccessor.HttpContext.RequestServices.GetService<ILocalizationService>();

                    foreach (var culture in await localizationService.GetSupportedCulturesAsync())
                    {
                        var cultureInfo = CultureInfo.GetCultureInfo(culture);
                        var shape = await shapeFactory.CreateAsync("CulturePickerItem", Arguments.From(new
                        {
                            Text = cultureInfo.DisplayName,
                            Value = cultureInfo.Name
                        }));

                        culturePicker.Add(shape);
                    }
                });
            builder.Describe("CulturePickerItem")
                .OnProcessing(context =>
                {
                    var culturePickerItem = context.Shape;
                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var selectedCulture = httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;

                    if (culturePickerItem.Value == selectedCulture)
                    {
                        culturePickerItem.Attributes.Add("selected", "selected");
                    }
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