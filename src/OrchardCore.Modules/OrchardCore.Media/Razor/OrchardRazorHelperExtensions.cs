using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore;
using OrchardCore.Media;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

#pragma warning disable CA1050 // Declare types in namespaces
public static class OrchardRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns the relative URL of the specified asset path with optional resizing parameters.
    /// </summary>
    public static string AssetUrl(this IOrchardHelper orchardHelper, string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return assetPath;
        }

        var resolvedAssetPath = mediaFileStore.MapPathToPublicUrl(assetPath);

        if (appendVersion)
        {
            var fileVersionProvider = orchardHelper.HttpContext.RequestServices.GetService<IFileVersionProvider>();

            resolvedAssetPath = fileVersionProvider.AddFileVersionToPath(orchardHelper.HttpContext.Request.PathBase, resolvedAssetPath);
        }

        return orchardHelper.ImageResizeUrl(resolvedAssetPath, width, height, resizeMode, quality, format, anchor, bgcolor);
    }

    /// <summary>
    /// Returns the relative URL of the specified asset path for a media profile with optional resizing parameters.
    /// </summary>
    public static Task<string> AssetProfileUrlAsync(this IOrchardHelper orchardHelper, string assetPath, string imageProfile, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
    {
        var mediaFileStore = orchardHelper.HttpContext.RequestServices.GetService<IMediaFileStore>();

        if (mediaFileStore == null)
        {
            return Task.FromResult(assetPath);
        }

        var resolvedAssetPath = mediaFileStore.MapPathToPublicUrl(assetPath);

        if (appendVersion)
        {
            var fileVersionProvider = orchardHelper.HttpContext.RequestServices.GetService<IFileVersionProvider>();

            resolvedAssetPath = fileVersionProvider.AddFileVersionToPath(orchardHelper.HttpContext.Request.PathBase, resolvedAssetPath);
        }

        return orchardHelper.ImageProfileResizeUrlAsync(resolvedAssetPath, imageProfile, width, height, resizeMode, quality, format, anchor, bgcolor);
    }

    /// <summary>
    /// Returns a URL with custom resizing parameters for an existing image path.
    /// </summary>
    public static string ImageResizeUrl(this IOrchardHelper orchardHelper, string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
    {
        var resizedUrl = ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, null, width, height, resizeMode, quality, format, anchor, bgcolor);

        return orchardHelper.TokenizeUrl(resizedUrl);
    }

    /// <summary>
    /// Returns a URL with custom resizing parameters for a media profile for an existing image path.
    /// </summary>
    public static async Task<string> ImageProfileResizeUrlAsync(this IOrchardHelper orchardHelper, string imagePath, string imageProfile, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
    {
        var mediaProfileService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IMediaProfileService>();
        var queryStringParams = await mediaProfileService.GetMediaProfileCommands(imageProfile);

        var resizedUrl = ImageSharpUrlFormatter.GetImageResizeUrl(imagePath, queryStringParams, width, height, resizeMode, quality, format, anchor, bgcolor);

        return orchardHelper.TokenizeUrl(resizedUrl);
    }

    private static string TokenizeUrl(this IOrchardHelper orchardHelper, string url)
    {
        var mediaOptions = orchardHelper.HttpContext.RequestServices.GetService<IOptions<MediaOptions>>().Value;
        if (mediaOptions.UseTokenizedQueryString)
        {
            var mediaTokenService = orchardHelper.HttpContext.RequestServices.GetService<IMediaTokenService>();

            url = mediaTokenService.AddTokenToPath(url);
        }

        return url;
    }
}
