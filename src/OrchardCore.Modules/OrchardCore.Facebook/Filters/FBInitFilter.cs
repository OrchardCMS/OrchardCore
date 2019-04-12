using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Filters
{
    public class FBInitFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly ISiteService _siteService;

        public FBInitFilter(
            IResourceManager resourceManager,
            ISiteService siteService)
        {
            _resourceManager = resourceManager;
            _siteService = siteService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                var site = (await _siteService.GetSiteSettingsAsync());
                var settings = site.As<FacebookSettings>();
                if (!string.IsNullOrWhiteSpace(settings?.AppId))
                {
                    if (settings.FBInit)
                    {
                        var options = $"{{ appId:'{settings.AppId}',version:'{settings.Version}'";
                        if (string.IsNullOrWhiteSpace(settings.FBInitParams))
                        {
                            options = string.Concat(options, "}");
                        }
                        else
                        {
                            options = string.Concat(options, ",", settings.FBInitParams, "}");
                        }
                        _resourceManager.RegisterHeadScript(new HtmlString($"<script>window.fbAsyncInit = function(){{ FB.init({options});}};</script>"));
                    }
                    var locale = string.IsNullOrWhiteSpace(site.Culture) ? "en_US" : site.Culture.Replace("-", "_");
                    _resourceManager.RegisterHeadScript(new HtmlString($"<script async defer src=\"https://connect.facebook.net/{locale}/sdk.js\"></script>"));

                }
            }
            await next.Invoke();
        }
    }
}