using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject an instance of <see cref="ISite"/> in the HttpContext items such that
    /// a View can reuse it when it's executed.
    /// TODO: This should be removed once https://github.com/aspnet/Mvc/issues/8241 is implemented
    /// </summary>
    public class SiteViewResultFilter : IAsyncResultFilter
    {
        private readonly ISiteService _siteService;

        public SiteViewResultFilter(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {

            if (!context.HttpContext.Items.ContainsKey(typeof(ISite)))
            {
                context.HttpContext.Items.Add(typeof(ISite), await _siteService.GetSiteSettingsAsync());
            }

            await next();
        }
    }
}
