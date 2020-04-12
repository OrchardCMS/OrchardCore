using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class PageTitleShapes : IShapeAttributeProvider
    {
        private IPageTitleBuilder _pageTitleBuilder;

        public IPageTitleBuilder Title
        {
            get
            {
                if (_pageTitleBuilder == null)
                {
                    _pageTitleBuilder = ShellScope.Services.GetRequiredService<IPageTitleBuilder>();
                }

                return _pageTitleBuilder;
            }
        }

        [Shape]
        public async Task<IHtmlContent> PageTitle(IHtmlHelper Html)
        {
            var siteSettings = await ShellScope.Services.GetRequiredService<ISiteService>().GetSiteSettingsAsync();

            // We must return a page title so if the format setting is blank just use the current title unformatted
            if (String.IsNullOrWhiteSpace(siteSettings.PageTitleFormat))
            {
                return Title.GenerateTitle(null);
            }
            else
            {
                var liquidTemplateManager = ShellScope.Services.GetRequiredService<ILiquidTemplateManager>();
                var htmlEncoder = ShellScope.Services.GetRequiredService<HtmlEncoder>();

                var result = await liquidTemplateManager.RenderAsync(siteSettings.PageTitleFormat, htmlEncoder);
                return new HtmlString(result);
            }
        }
    }
}
