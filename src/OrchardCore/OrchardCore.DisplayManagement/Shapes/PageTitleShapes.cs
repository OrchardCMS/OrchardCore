using System;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Liquid;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Shapes
{

    public class PageTitleShapes : IShapeAttributeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public PageTitleShapes(IHttpContextAccessor httpContextAccessor, ISiteService siteService, ILiquidTemplateManager liquidTemplateManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
        }

        private IPageTitleBuilder _pageTitleBuilder;
        public IPageTitleBuilder Title
        {
            get
            {
                if (_pageTitleBuilder == null)
                {
                    _pageTitleBuilder = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IPageTitleBuilder>();
                }

                return _pageTitleBuilder;
            }
        }

        [Shape]
        public async Task<IHtmlContent> PageTitle(IHtmlHelper Html)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // We must return a page title so if the format setting is blank just use the current title unformatted
            if (String.IsNullOrWhiteSpace(siteSettings.PageTitleFormat))
            {
                return Title.GenerateTitle(null);
            }
            else
            {
                return GenerateTitleUsingFormat(siteSettings.PageTitleFormat);
            }
        }

        private IHtmlContent GenerateTitleUsingFormat(string pageTitleFormat)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            // Need to parse the title format somehow
            //var templateContext = new TemplateContext();

            //var test = await _liquidTemplateManager.RenderAsync(pageTitleFormat, NullEncoder.Default, templateContext);

            htmlContentBuilder.AppendHtml(pageTitleFormat);

            return htmlContentBuilder;
        }
    }
}
