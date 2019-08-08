using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore;
using OrchardCore.ResourceManagement;

public static class ResourceCdnHelperExtensions
{
    /// <summary>
    /// Returns the Cdn Base Url of the specified resource path.
    /// </summary>
    public static string ResourceUrl(this IOrchardHelper orchardHelper, string resourcePath)
    {
        var options = orchardHelper.HttpContext.RequestServices.GetRequiredService<IOptions<ResourceManagementOptions>>().Value;

        if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
        {
            if (!String.IsNullOrEmpty(orchardHelper.HttpContext.Request.PathBase))
            {
                resourcePath = orchardHelper.HttpContext.Request.PathBase + resourcePath.Substring(1);
            }
            else
            {
                resourcePath = resourcePath.Substring(1);
            }
        }

        // Don't prefix cdn if the path is absolute, or is in debug mode.
        if (!options.DebugMode
            && !String.IsNullOrEmpty(options.CdnBaseUrl)
            && !Uri.TryCreate(resourcePath, UriKind.Absolute, out var uri))
        {
            resourcePath = options.CdnBaseUrl + resourcePath;
        }

        return resourcePath;
    }
}
