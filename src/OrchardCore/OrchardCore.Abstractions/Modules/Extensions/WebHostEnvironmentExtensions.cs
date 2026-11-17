using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Modules;

public static class WebHostEnvironmentExtensions
{
    /// <summary>
    /// Gets the absolute path of the web root, or the <c>wwwroot</c> folder under the content root
    /// when the host has no web root, e.g. when an empty <c>wwwroot</c> folder is not published.
    /// The returned folder may not exist.
    /// </summary>
    public static string GetWebRootPathOrDefault(this IWebHostEnvironment environment)
        => string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, Module.WebRootPath)
            : environment.WebRootPath;
}
