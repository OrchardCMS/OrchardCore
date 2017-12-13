using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media;

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
    }
}
