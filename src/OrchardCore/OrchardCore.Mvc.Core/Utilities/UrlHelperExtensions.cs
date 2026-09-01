using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Core.Utilities;

public static class UrlHelperExtensions
{
    public static string ToAbsoluteAction(this IUrlHelper url, string actionName, string controllerName, object routeValues = null)
    {
        return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
    }

    public static string GetBaseUrl(this IUrlHelper url)
    {
        var request = url.ActionContext.HttpContext.Request;
        var scheme = request.Scheme;
        var host = request.Host.ToUriComponent();
        return $"{scheme}://{host}";
    }

    public static string ToAbsoluteUrl(this IUrlHelper url, string virtualPath)
    {
        // The virtual path may already be an absolute URL, e.g. when media is served from a
        // CDN (IMediaFileStore.MapPathToPublicUrl() prefixes the CDN base URL). In that case,
        // prefixing it with the site's own base URL would produce an invalid, concatenated URL.
        // Note: Uri.TryCreate(..., UriKind.Absolute, ...) also accepts rooted paths like
        // "/media/image.jpg" as a valid "file://" URI, so the scheme must be checked explicitly.
        if (Uri.TryCreate(virtualPath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return virtualPath;
        }

        var baseUrl = url.GetBaseUrl();
        var path = url.Content(virtualPath);

        return $"{baseUrl}{path}";
    }
}
