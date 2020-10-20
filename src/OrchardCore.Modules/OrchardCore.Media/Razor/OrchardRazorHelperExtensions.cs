using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Media;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

public static class OrchardRazorHelperExtensions
{
    /// <summary>
    /// Returns the relative URL of the specified asset path with optional resizing parameters.
    /// </summary>
    public static string AssetUrl(this IOrchardHelper orchardHelper, string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false, int? quality = null, Format format = Format.Undefined)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return assetPath;
        }

        var resolvedAssetPath = mediaFileStore.MapPathToPublicUrl(assetPath);

        var resizedUrl = orchardHelper.ImageResizeUrl(resolvedAssetPath, width, height, resizeMode, quality, format);

        if (appendVersion)
        {
            var fileVersionProvider = orchardHelper.HttpContext.RequestServices.GetService<IFileVersionProvider>();

            resizedUrl = fileVersionProvider.AddFileVersionToPath(orchardHelper.HttpContext.Request.PathBase, resizedUrl);
        }

        return resizedUrl;
    }

    /// <summary>
    /// Returns the relative URL of the specified asset path for a media profile with optional resizing parameters.
    /// </summary>
    public static async Task<string> AssetProfileUrlAsync(this IOrchardHelper orchardHelper, string assetPath, string imageProfile, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false, int? quality = null, Format format = Format.Undefined)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return assetPath;
        }

        var resolvedAssetPath = mediaFileStore.MapPathToPublicUrl(assetPath);

        var resizedUrl = await orchardHelper.ImageProfileResizeUrlAsync(resolvedAssetPath, imageProfile, width, height, resizeMode, quality, format);

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
    public static string ImageResizeUrl(this IOrchardHelper orchardHelper, string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined)
    {
        return ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, null, width, height, resizeMode, quality, format);
    }

    /// <summary>
    /// Returns a URL with custom resizing parameters for a media profile for an existing image path.
    /// </summary>
    public static async Task<string> ImageProfileResizeUrlAsync(this IOrchardHelper orchardHelper, string imagePath, string imageProfile, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined)
    {
        var mediaProfileService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IMediaProfileService>();
        var queryStringParams = await mediaProfileService.GetMediaProfileCommands(imageProfile);

        return ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, queryStringParams, width, height, resizeMode, quality, format);
    }
}
