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
                        var require = new RequireSettings() { Name = "fb", Type = "script", };
                        var resource=_resourceManager.FindResource(require);

                        _resourceManager.RegisterResource("script", "fb");

                        _resourceManager.RegisterFootScript(new HtmlString(@"<script src=""/OrchardCore.Facebook/sdk/fb.js"" type=""text/javascript""></script>"));
                        _resourceManager.RegisterFootScript(new HtmlString(@"<script src=""/OrchardCore.Facebook/sdk/fbsdk.js"" type=""text/javascript""></script>"));
                    }
                }
            }
            await next.Invoke();
        }
    }
}