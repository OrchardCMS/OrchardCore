using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    /// <summary>
    /// Returns the Cdn Base Url of the specified resource path.
    /// </summary>
    public class ResourceUrlFilter : ILiquidFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;

        public ResourceUrlFilter(IHttpContextAccessor httpContextAccessor, IOptions<ResourceManagementOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var resourcePath = input.ToStringValue();

            if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourcePath = _httpContextAccessor.HttpContext.Request.PathBase.Add(resourcePath[1..]).Value;
            }

            // Don't prefix cdn if the path includes a protocol, i.e. is an external url, or is in debug mode.
            if (!_options.DebugMode && !String.IsNullOrEmpty(_options.CdnBaseUrl) &&
                // Don't evaluate with Uri.TryCreate as it produces incorrect results on Linux.
                !resourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("//", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = _options.CdnBaseUrl + resourcePath;
            }

            return new ValueTask<FluidValue>(new StringValue(resourcePath));
        }
    }
}
