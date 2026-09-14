using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Core;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Describes the breadcrumb trails of the rate limits screens.
/// </summary>
public sealed class RateLimitsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.RateLimits" },
    };

    internal readonly IStringLocalizer S;

    public RateLimitsBreadcrumbProvider(IStringLocalizer<RateLimitsBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case RateLimitsBreadcrumbs.List:
                AddList(builder);
                break;

            case RateLimitsBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["New Rate Limit Policy"], item => item.Id("Policy"));
                break;

            case RateLimitsBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Rate Limit Policy"], item => item.Id("Policy"));
                break;

            case RateLimitsBreadcrumbs.LimiterCreate:
                AddList(builder);
                builder.Add(S["Add '{0}' Limiter", builder.GetData<string>(RateLimitsBreadcrumbs.DisplayNameKey)],
                    item => item.Id("Limiter"));
                break;

            case RateLimitsBreadcrumbs.LimiterEdit:
                AddList(builder);
                builder.Add(S["Edit '{0}' Limiter", builder.GetData<string>(RateLimitsBreadcrumbs.DisplayNameKey)],
                    item => item.Id("Limiter"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Rate Limit Policies"], item => item
            .Id("RateLimits")
            .Action("Index", "Admin", s_routeValues)
            .Permission(RateLimitsPermissions.ManageRateLimits));
}
