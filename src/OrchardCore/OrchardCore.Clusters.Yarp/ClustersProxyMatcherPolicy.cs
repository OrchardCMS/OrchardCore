using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Model;

namespace OrchardCore.Clusters;

/// <summary>
/// Only selects the tenant clusters 'RouteTemplate' for the requests that this instance should forward.
/// This is evaluated on each request, so it can't get out of sync with <see cref="ClustersOptions"/>
/// when the configuration is reloaded.
/// </summary>
public sealed class ClustersProxyMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    private readonly IOptionsMonitor<ClustersOptions> _optionsMonitor;

    public ClustersProxyMatcherPolicy(IOptionsMonitor<ClustersOptions> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public override int Order => 0;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) => endpoints.Any(IsRouteTemplate);

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        if (httpContext.AsClustersProxy(_optionsMonitor.CurrentValue))
        {
            return Task.CompletedTask;
        }

        for (var i = 0; i < candidates.Count; i++)
        {
            if (IsRouteTemplate(candidates[i].Endpoint))
            {
                candidates.SetValidity(i, false);
            }
        }

        return Task.CompletedTask;
    }

    internal static bool IsRouteTemplate(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<RouteModel>()?.Config.RouteId == ClustersOptions.RouteTemplate;
}
