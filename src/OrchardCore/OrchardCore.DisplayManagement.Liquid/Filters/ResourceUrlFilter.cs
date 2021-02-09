using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    /// <summary>
    /// Returns the Cdn Base Url of the specified resource path.
    /// </summary>
    public static class ResourceUrlFilter
    {
        public static ValueTask<FluidValue> ResourceUrl(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var httpContextAccessor = context.Services.GetRequiredService< IHttpContextAccessor>();
            var options = context.Services.GetRequiredService<ResourceManagementOptions>();

            var resourcePath = input.ToStringValue();

            if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourcePath = httpContextAccessor.HttpContext.Request.PathBase.Add(resourcePath.Substring(1)).Value;
            }

            // Don't prefix cdn if the path includes a protocol, i.e. is an external url, or is in debug mode.
            if (!options.DebugMode && !String.IsNullOrEmpty(options.CdnBaseUrl) &&
                // Don't evaluate with Uri.TryCreate as it produces incorrect results on Linux.
                !resourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("//", StringComparison.OrdinalIgnoreCase) &&
                !resourcePath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = options.CdnBaseUrl + resourcePath;
            }

            return new ValueTask<FluidValue>(new StringValue(resourcePath));
        }
    }
}
