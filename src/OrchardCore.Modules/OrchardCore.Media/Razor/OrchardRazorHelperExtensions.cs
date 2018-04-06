using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media;
using OrchardCore.Media.Processing;

namespace OrchardCore.DisplayManagement.Razor
{
    public static class OrchardRazorHelperExtensions
    {
        public static string AssetUrl(this OrchardRazorHelper razorHelper, string assetPath)
        {
            var mediaFileStore = razorHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

            if (mediaFileStore == null)
            {
                return assetPath;
            }

            return mediaFileStore.MapPathToPublicUrl(assetPath);
        }

        public static string AssetUrl(this OrchardRazorHelper razorHelper, string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
        {
            var resolvedAssetPath = razorHelper.AssetUrl(assetPath);
            return razorHelper.ImageResizeUrl(resolvedAssetPath, width, height, resizeMode);
        }

        public static string ImageResizeUrl(this OrchardRazorHelper razorHelper, string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
        {
            return ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, width, height, resizeMode);
        }
    }
}
