using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

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

    private readonly IRateLimitPolicyStore _policyStore;

    internal readonly IStringLocalizer S;

    public RateLimitsBreadcrumbProvider(IRateLimitPolicyStore policyStore, IStringLocalizer<RateLimitsBreadcrumbProvider> stringLocalizer)
    {
        _policyStore = policyStore;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case RateLimitsConstants.List:
                AddList(builder);
                break;

            case RateLimitsConstants.Create:
                AddList(builder);
                builder.Add(S["New Rate Limit Policy"], item => item.Id("Policy"));
                break;

            case RateLimitsConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit Rate Limit Policy"], item => item.Id("Policy"));
                break;

            case RateLimitsConstants.LimiterCreate:
                AddList(builder);
                await AddPolicyAsync(builder);
                builder.Add(S["Add '{0}' Limiter", builder.GetData<string>(RateLimitsConstants.DisplayNameKey)],
                    item => item.Id("Limiter"));
                break;

            case RateLimitsConstants.LimiterEdit:
                AddList(builder);
                await AddPolicyAsync(builder);
                builder.Add(S["Edit '{0}' Limiter", builder.GetData<string>(RateLimitsConstants.DisplayNameKey)],
                    item => item.Id("Limiter"));
                break;
        }
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Rate Limit Policies"], item => item
            .Id("RateLimits")
            .Action("Index", "Admin", s_routeValues)
            .Permission(RateLimitsPermissions.ManageRateLimits));

    // A limiter is only reached from its policy, so its trail leads back through the policy.
    private async ValueTask AddPolicyAsync(BreadcrumbBuilder builder)
    {
        var policyId = builder.GetData<string>(RateLimitsConstants.PolicyIdKey);

        if (string.IsNullOrEmpty(policyId))
        {
            return;
        }

        var policy = await _policyStore.FindByIdAsync(policyId, PolicyVersion.Current);

        builder.Add(policy?.Name ?? S["Rate Limit Policy"].Value, item => item
            .Id("Policy")
            .Action("Edit", "Admin", new RouteValueDictionary(s_routeValues)
            {
                { "policyId", policyId },
            })
            .Permission(RateLimitsPermissions.ManageRateLimits));
    }
}
