using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings.Recipes
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
            if (!string.Equals(context.Name, "Settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step;
            var site = await _siteService.LoadSiteSettingsAsync();

            foreach (var (name, value) in model)
            {
                if (value == null) continue;

                switch (name)
                {
                    case "BaseUrl":
                        site.BaseUrl = value.GetValue<string>();
                        break;

                    case "Calendar":
                        site.Calendar = value.GetValue<string>();
                        break;

                    case "MaxPagedCount":
                        site.MaxPagedCount = value.GetValue<int>();
                        break;

                    case "MaxPageSize":
                        site.MaxPageSize = value.GetValue<int>();
                        break;

                    case "PageSize":
                        site.PageSize = value.GetValue<int>();
                        break;

                    case "ResourceDebugMode":
                        site.ResourceDebugMode = (ResourceDebugMode)value.GetValue<int>();
                        break;

                    case "SiteName":
                        site.SiteName = value.GetValue<string>();
                        break;

                    case "PageTitleFormat":
                        site.PageTitleFormat = value.GetValue<string>();
                        break;

                    case "SiteSalt":
                        site.SiteSalt = value.GetValue<string>();
                        break;

                    case "SuperUser":
                        site.SuperUser = value.GetValue<string>();
                        break;

                    case "TimeZoneId":
                        site.TimeZoneId = value.GetValue<string>();
                        break;

                    case "UseCdn":
                        site.UseCdn = value.GetValue<bool>();
                        break;

                    case "CdnBaseUrl":
                        site.CdnBaseUrl = value.GetValue<string>();
                        break;

                    case "AppendVersion":
                        site.AppendVersion = value.GetValue<bool>();
                        break;

                    case "HomeRoute":
                        site.HomeRoute = value.Deserialize<RouteValueDictionary>();
                        break;

                    case "CacheMode":
                        site.CacheMode = (CacheMode)value.GetValue<int>();
                        break;

                    default:
                        site.Properties[name] = value;
                        break;
                }
            }

            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}
