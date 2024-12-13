using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Media.Services;

public sealed class MediaOptionsConfiguration : IConfigureOptions<MediaOptions>
{
    private static readonly int[] _defaultSupportedSizes = [16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048];

    private static readonly string[] _defaultAllowedFileExtensions = [
        // Images
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".ico",
        ".svg",
        ".webp",

        // Documents
        ".pdf", // (Portable Document Format; Adobe Acrobat)
        ".doc",
        ".docx", // (Microsoft Word Document)
        ".ppt",
        ".pptx",
        ".pps",
        ".ppsx", // (Microsoft PowerPoint Presentation)
        ".odt", // (OpenDocument Text Document)
        ".xls",
        ".xlsx", // (Microsoft Excel Document)
        ".psd", // (Adobe Photoshop Document)

        // Audio
        ".mp3",
        ".m4a",
        ".ogg",
        ".wav",

        // Video
        ".mp4",
        ".m4v", // (MPEG-4)
        ".mov", // (QuickTime)
        ".wmv", // (Windows Media Video)
        ".avi",
        ".mpg",
        ".ogv", // (Ogg)
        ".3gp", // (3GPP)
        ".webm",
    ];

    private const int DefaultMaxBrowserCacheDays = 30;
    private const int DefaultSecureFilesMaxBrowserCacheDays = 0;
    private const int DefaultMaxCacheDays = 365;
    private const int DefaultMaxFileSize = 30_000_000;

    private const string DefaultAssetsPath = "Media";
    private const string DefaultAssetsUsersFolder = "_Users";
    private const string DefaultAssetsRequestPath = "/media";

    private const bool DefaultUseTokenizedQueryString = true;

    // default-src self applied to prevent possible svg xss injection.
    // style-src applied to allow browser behaviour of wrapping raw images in a styled img element.
    private const string DefaultContentSecurityPolicy = "default-src 'self'; style-src 'unsafe-inline'";

    private const int DefaultMaxUploadChunkSize = 104_857_600; // 100MB

    private static readonly TimeSpan _defaultTemporaryFileLifeTime = TimeSpan.FromHours(1);

    private readonly IShellConfiguration _shellConfiguration;

    public MediaOptionsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(MediaOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Media");

        // Because IShellConfiguration treats arrays as key value pairs, we replace the array value,
        // rather than letting Configure merge the default array with the appsettings value.
        options.SupportedSizes = section.GetSection("SupportedSizes")
            .Get<int[]>()?.OrderBy(s => s).ToArray() ?? _defaultSupportedSizes;

        options.AllowedFileExtensions = new HashSet<string>(
            section.GetSection("AllowedFileExtensions").Get<string[]>() ?? _defaultAllowedFileExtensions,
            StringComparer.OrdinalIgnoreCase);

        options.MaxBrowserCacheDays = section.GetValue("MaxBrowserCacheDays", DefaultMaxBrowserCacheDays);
        options.MaxSecureFilesBrowserCacheDays = section.GetValue("MaxSecureFilesBrowserCacheDays", DefaultSecureFilesMaxBrowserCacheDays);
        options.MaxCacheDays = section.GetValue("MaxCacheDays", DefaultMaxCacheDays);
        options.ResizedCacheMaxStale = section.GetValue<TimeSpan?>(nameof(options.ResizedCacheMaxStale));
        options.RemoteCacheMaxStale = section.GetValue<TimeSpan?>(nameof(options.RemoteCacheMaxStale));
        options.MaxFileSize = section.GetValue("MaxFileSize", DefaultMaxFileSize);
        options.CdnBaseUrl = section.GetValue("CdnBaseUrl", string.Empty).TrimEnd('/').ToLower();
        options.AssetsRequestPath = section.GetValue("AssetsRequestPath", DefaultAssetsRequestPath);
        options.AssetsPath = section.GetValue("AssetsPath", DefaultAssetsPath);
        options.AssetsUsersFolder = section.GetValue("AssetsUsersFolder", DefaultAssetsUsersFolder);
        options.UseTokenizedQueryString = section.GetValue("UseTokenizedQueryString", DefaultUseTokenizedQueryString);
        options.MaxUploadChunkSize = section.GetValue(nameof(options.MaxUploadChunkSize), DefaultMaxUploadChunkSize);
        options.TemporaryFileLifetime = section.GetValue(nameof(options.TemporaryFileLifetime), _defaultTemporaryFileLifeTime);

        var contentSecurityPolicy = section.GetValue("ContentSecurityPolicy", DefaultContentSecurityPolicy);

        // Use the same cache control header as ImageSharp does for resized images.
        var cacheControl = "public, must-revalidate, max-age=" + TimeSpan.FromDays(options.MaxBrowserCacheDays).TotalSeconds.ToString();
        // Secure files are not cached at all.
        var secureCacheControl = options.MaxSecureFilesBrowserCacheDays == 0
            ? "no-store"
            : "public, must-revalidate, max-age=" + TimeSpan.FromDays(options.MaxSecureFilesBrowserCacheDays).TotalSeconds.ToString();

        options.StaticFileOptions = new StaticFileOptions
        {
            RequestPath = options.AssetsRequestPath,
            ServeUnknownFileTypes = true,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = ctx.Context.IsSecureMediaRequested() ? secureCacheControl : cacheControl;
                ctx.Context.Response.Headers.ContentSecurityPolicy = contentSecurityPolicy;
            }
        };
    }
}
