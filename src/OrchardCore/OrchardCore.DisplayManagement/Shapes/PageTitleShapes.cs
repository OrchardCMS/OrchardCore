using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Shapes
{
    
    public class PageTitleShapes : IShapeAttributeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        public PageTitleShapes(IHttpContextAccessor httpContextAccessor, ISiteService siteService, IStringLocalizer<PageTitleShapes> localizer, IHtmlLocalizer<PageTitleShapes> htmlLocalizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
            T = localizer;
            H = htmlLocalizer;
        }

        IStringLocalizer T { get; }
        IHtmlLocalizer H { get; }


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
            if(String.IsNullOrWhiteSpace(siteSettings.PageTitleFormat))
            {
                return Title.GenerateTitle(null);
            }

            //TODO: parse the page title format with liquid to construct the users required page title format
            // for now just use my site setting as is for feedback
            return Html.Raw(siteSettings.PageTitleFormat); ;
        }
    }
}
