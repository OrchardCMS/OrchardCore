using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Settings.Recipes
{
    public class SettingsStep : RecipeExecutionStep
    {
        private readonly ISiteService _siteService;

        public SettingsStep(
            ISiteService siteService,
            ILoggerFactory logger,
            IStringLocalizer<ISiteService> localizer) : base(logger, localizer)
        {
            _siteService = siteService;
        }

        public override string Name
        {
            get { return "Settings"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step;
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

            if (model["PageTitleSeparator"] != null)
            {
                site.PageTitleSeparator = model["PageTitleSeparator"].ToString();
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