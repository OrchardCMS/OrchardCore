using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Media;
using OrchardCore.Media.Processing;

public static class OrchardRazorHelperExtensions
{
    /// <summary>
    /// Returns the relative URL of the specifier asset path with optional resizing parameters.
    /// </summary>
    public static string AssetUrl(this IOrchardHelper orchardHelper, string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return assetPath;
        }

        var resolvedAssetPath = mediaFileStore.MapPathToPublicUrl(assetPath);

        var resizedUrl = orchardHelper.ImageResizeUrl(resolvedAssetPath, width, height, resizeMode);

        if (appendVersion)
        {
            var fileVersionProvider = orchardHelper.HttpContext.RequestServices.GetService<IFileVersionProvider>();

            resizedUrl = fileVersionProvider.AddFileVersionToPath(orchardHelper.HttpContext.Request.PathBase, resizedUrl);
        }
        return resizedUrl;
    }

    /// <summary>
    /// Returns a URL with custom resizing parameters for an existing image path.
    /// </summary>
    public static string ImageResizeUrl(this IOrchardHelper orchardHelper, string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
    {
        return ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, width, height, resizeMode);
    }
}