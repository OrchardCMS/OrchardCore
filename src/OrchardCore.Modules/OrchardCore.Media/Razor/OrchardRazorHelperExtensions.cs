using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Media;
using OrchardCore.Media.Processing;

public static class OrchardRazorHelperExtensions
{
    public static string AssetUrl(this IOrchardHelper orchardHelper, string assetPath)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return assetPath;
        }

        return mediaFileStore.MapPathToPublicUrl(assetPath);
    }

    public static string AssetUrl(this IOrchardHelper orchardHelper, string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
    {
        var resolvedAssetPath = orchardHelper.AssetUrl(assetPath);
        return orchardHelper.ImageResizeUrl(resolvedAssetPath, width, height, resizeMode);
    }

    public static string ImageResizeUrl(this IOrchardHelper orchardHelper, string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
    {
        return ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, width, height, resizeMode);
    }
}