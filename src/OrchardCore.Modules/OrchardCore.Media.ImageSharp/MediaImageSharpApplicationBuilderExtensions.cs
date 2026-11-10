using SixLabors.ImageSharp.Web;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Adds the ImageSharp based media image processing middleware.
/// </summary>
public static class MediaImageSharpApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the middleware resizing the media images. It must be added before the static files
    /// middleware serving the media files.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static IApplicationBuilder UseMediaImageSharp(this IApplicationBuilder app)
        => app.UseImageSharp();
}
