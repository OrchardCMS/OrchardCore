using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Settings.Recipes
{
    /// <summary>
    /// This recipe step updates the site settings.
    /// </summary>
    public class SettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public SettingsStep(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step;
            var site = await _siteService.GetSiteSettingsAsync();

            if (model["BaseUrl"] != null)
            {
                site.BaseUrl = model["BaseUrl"].ToString();
            }

            if (model["Calendar"] != null)
            {
                site.Calendar = model["Calendar"].ToString();
            }

            if (model["Culture"] != null)
            {
                site.Culture = model["Culture"].ToString();
            }

            if (model["MaxPagedCount"] != null)
            {
                site.MaxPagedCount = model.Value<int>("MaxPagedCount");
            }

            if (model["MaxPageSize"] != null)
            {
                site.MaxPageSize = model.Value<int>("MaxPageSize");
            }

            if (model["PageSize"] != null)
            {
                site.PageSize = model.Value<int>("PageSize");
            }

            if (model["ResourceDebugMode"] != null)
            {
                site.ResourceDebugMode = model.Value<ResourceDebugMode>("ResourceDebugMode");
            }

            if (model["SiteName"] != null)
            {
                site.SiteName = model["SiteName"].ToString();
            }

            if (model["SiteSalt"] != null)
            {
                site.SiteSalt = model["SiteSalt"].ToString();
            }

            if (model["SuperUser"] != null)
            {
                site.SuperUser = model["SuperUser"].ToString();
            }

            if (model["TimeZone"] != null)
            {
                site.TimeZone = model["TimeZone"].ToString();
            }

            if (model["UseCdn"] != null)
            {
                site.UseCdn = model.Value<bool>("UseCdn");
            }

            if (model["HomeRoute"] != null)
            {
                site.HomeRoute = model["HomeRoute"].ToObject<RouteValueDictionary>();
            }

            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}