using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Rules.Models;
using OrchardCore.Settings;

namespace OrchardCore.Rules.Services;

public class HomepageConditionEvaluator : ConditionEvaluator<HomepageCondition>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AutorouteOptions _autorouteOptions;
    private readonly ISiteService _siteService;

    public HomepageConditionEvaluator(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AutorouteOptions> autorouteOptions,
        ISiteService siteService)
    {
        _httpContextAccessor = httpContextAccessor;
        _autorouteOptions = autorouteOptions.Value;
        _siteService = siteService;
    }

    public override async ValueTask<bool> EvaluateAsync(HomepageCondition condition)
    {
        var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

        if (condition.Value && string.Equals("/", requestPath, StringComparison.Ordinal) || string.IsNullOrEmpty(requestPath))
        {
            return true;
        }

        if (_httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(_autorouteOptions.ContentItemIdKey, out var contentItemId))
        {
            var values = new RouteValueDictionary(_autorouteOptions.GlobalRouteValues)
            {
                { _autorouteOptions.ContentItemIdKey, contentItemId },
            };

            var settings = await _siteService.GetSiteSettingsAsync();

            if (AreRouteValueDictionariesEqual(settings.HomeRoute, values))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AreRouteValueDictionariesEqual(RouteValueDictionary dict1, RouteValueDictionary dict2)
    {
        if (dict1.Count != dict2.Count)
        {
            return false;
        }

        // Check if the dictionaries have the same keys and values
        return dict1.All(kvp => dict2.TryGetValue(kvp.Key, out var value) && kvp.Value.Equals(value));
    }
}
