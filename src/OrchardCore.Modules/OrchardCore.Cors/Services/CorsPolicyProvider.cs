using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Cors.Services;

/// <summary>
/// Provides the CORS policies of the current <see cref="CorsOptions"/>, as the default provider
/// keeps the options it is first built with.
/// </summary>
internal sealed class CorsPolicyProvider : ICorsPolicyProvider
{
    private readonly IOptionsMonitor<CorsOptions> _corsOptions;

    public CorsPolicyProvider(IOptionsMonitor<CorsOptions> corsOptions)
    {
        _corsOptions = corsOptions;
    }

    public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
    {
        var options = _corsOptions.CurrentValue;

        return Task.FromResult(options.GetPolicy(policyName ?? options.DefaultPolicyName));
    }
}
