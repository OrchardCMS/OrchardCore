using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

internal sealed class MediaUploadOptionsConfiguration : IPostConfigureOptions<MediaOptions>
{
    private readonly ISiteService _siteService;

    public MediaUploadOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void PostConfigure(string name, MediaOptions options) => Apply(options, _siteService.GetSettings<MediaUploadSettings>());

    public static void Apply(MediaOptions options, MediaUploadSettings settings)
    {
        if (settings.MaxFileSize is > 0)
        {
            options.MaxFileSize = Math.Min(options.MaxFileSize, settings.MaxFileSize.Value);
        }
        if (settings.AllowedFileExtensions is not null)
        {
            // Preserve the distinction: restricted extensions still require the
            // existing UploadRestrictedMedia permission in every upload path.
            options.AllowedFileExtensions.IntersectWith(settings.AllowedFileExtensions);
            options.RestrictedFileExtensions.IntersectWith(settings.AllowedFileExtensions);
        }
    }
}
