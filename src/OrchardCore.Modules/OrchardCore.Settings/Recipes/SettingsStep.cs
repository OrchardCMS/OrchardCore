using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings.Recipes;

/// <summary>
/// This recipe step updates the site settings.
/// </summary>
public sealed class SettingsStep : NamedRecipeStepHandler
{
    private readonly ISiteService _siteService;

    public SettingsStep(ISiteService siteService)
        : base("Settings")
    {
        _siteService = siteService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step;
        var site = await _siteService.LoadSiteSettingsAsync();

        foreach (var property in model)
        {
            switch (property.Key)
            {
                case "BaseUrl":
                    site.BaseUrl = property.Value.ToString();
                    break;

                case "Calendar":
                    site.Calendar = property.Value.ToString();
                    break;

                case "MaxPagedCount":
                    site.MaxPagedCount = property.Value.Value<int>();
                    break;

                case "MaxPageSize":
                    site.MaxPageSize = property.Value.Value<int>();
                    break;

                case "PageSize":
                    site.PageSize = property.Value.Value<int>();
                    break;

                case "ResourceDebugMode":
                    site.ResourceDebugMode = (ResourceDebugMode)property.Value.Value<int>();
                    break;

                case "SiteName":
                    site.SiteName = property.Value.ToString();
                    break;

                case "PageTitleFormat":
                    site.PageTitleFormat = property.Value.ToString();
                    break;

                case "SiteSalt":
                    site.SiteSalt = property.Value.ToString();
                    break;

                case "SuperUser":
                    site.SuperUser = property.Value.ToString();
                    break;

                case "TimeZoneId":
                    site.TimeZoneId = property.Value.ToString();
                    break;

                case "UseCdn":
                    site.UseCdn = property.Value.Value<bool>();
                    break;

                case "CdnBaseUrl":
                    site.CdnBaseUrl = property.Value.ToString();
                    break;

                case "AppendVersion":
                    site.AppendVersion = property.Value.Value<bool>();
                    break;

                case "HomeRoute":
                    site.HomeRoute = property.Value.ToObject<RouteValueDictionary>();
                    break;

                case "CacheMode":
                    site.CacheMode = (CacheMode)property.Value.Value<int>();
                    break;

                default:
                    site.Properties[property.Key] = property.Value.Clone();
                    break;
            }
        }

        await _siteService.UpdateSiteSettingsAsync(site);
    }
}
