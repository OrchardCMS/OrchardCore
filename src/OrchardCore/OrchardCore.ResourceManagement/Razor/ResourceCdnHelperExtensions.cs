using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore;
using OrchardCore.ResourceManagement;

public static class ResourceCdnHelperExtensions
{
    /// <summary>
    /// Prefixes the Cdn Base URL to the specified resource path.
    /// </summary>
    public static string ResourceUrl(this IOrchardHelper orchardHelper, string resourcePath, bool? appendVersion = null)
    {
        var options = orchardHelper.HttpContext.RequestServices.GetRequiredService<IOptions<ResourceManagementOptions>>().Value;
        var fileVersionProvider = orchardHelper.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();

        if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
        {
            resourcePath = orchardHelper.HttpContext.Request.PathBase.Add(resourcePath.Substring(1)).Value;
        }

        // If append version is set, allow it to override the site setting.
        if (resourcePath != null && ((appendVersion.HasValue && appendVersion == true) ||
                (!appendVersion.HasValue && options.AppendVersion == true)))
        {
            resourcePath = fileVersionProvider.AddFileVersionToPath(orchardHelper.HttpContext.Request.PathBase, resourcePath);
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
