using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings.Recipes;

/// <summary>
/// This recipe step updates the site settings.
/// </summary>
/// <remarks>
/// The options built from the site settings are refreshed once the settings are saved. The tenant is only
/// released when settings registered in <see cref="SiteSettingsReleaseOptions"/> are changed.
/// </remarks>
public sealed class SettingsStep : NamedRecipeStepHandler
{
    private readonly ISiteService _siteService;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly SiteSettingsReleaseOptions _siteSettingsReleaseOptions;

    public SettingsStep(
        ISiteService siteService,
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IShellReleaseManager shellReleaseManager,
        IOptions<SiteSettingsReleaseOptions> siteSettingsReleaseOptions)
        : base("Settings")
    {
        _siteService = siteService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _shellReleaseManager = shellReleaseManager;
        _siteSettingsReleaseOptions = siteSettingsReleaseOptions.Value;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step;
        if (model == null)
        {
            return;
        }

        var site = await _siteService.LoadSiteSettingsAsync();
        site ??= new SiteSettings();

        var releaseShell = false;

        foreach (var property in model)
        {
            if (property.Value is null)
            {
                continue;
            }

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

                case "AllowPageSizeSelection":
                    site.AllowPageSizeSelection = property.Value.Value<bool>();
                    break;

                case "PageSizeOptions":
                    site.PageSizeOptions = property.Value.ToObject<int[]>();
                    break;

                case "ResourceDebugMode":
                    if (property.Value.TryGetEnumValue<ResourceDebugMode>(out var resourceDebugMode))
                    {
                        site.ResourceDebugMode = resourceDebugMode.Value;
                    }
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
                    if (property.Value.TryGetEnumValue<CacheMode>(out var cacheMode))
                    {
                        site.CacheMode = cacheMode.Value;
                    }
                    break;

                default:
                    if (_siteSettingsReleaseOptions.Settings.Contains(property.Key) &&
                        !JsonNode.DeepEquals(site.Properties[property.Key], property.Value))
                    {
                        releaseShell = true;
                    }

                    site.Properties[property.Key] = property.Value.Clone();
                    break;
            }
        }

        await _siteService.UpdateSiteSettingsAsync(site);

        // Any options may be built from the updated settings.
        _optionsUpdateNotifier.RequestUpdateAll();

        if (releaseShell)
        {
            _shellReleaseManager.RequestRelease();
        }
    }
}
