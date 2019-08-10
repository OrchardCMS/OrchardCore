using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media
{
    public class MediaOptions
    {
        private static readonly PathString DefaultAssetsRequestPath = new PathString("/media");

        private static readonly string DefaultAssetsCachePath = "MediaCache";

        /// <summary>
        /// The request path used to route asset files, defaults to /media
        /// </summary>
        public PathString AssetsRequestPath = DefaultAssetsRequestPath;

        /// <summary>
        /// Specify the type of cache to use when resizing media, defaults to the Physical disc cache.
        /// </summary>

        public CacheConfiguration CacheConfiguration = CacheConfiguration.Physical;

        /// <summary>
        /// Specify the folder to stored cached assets, defaults to MediaCache.
        /// </summary>
        public string AssetsCachePath = DefaultAssetsCachePath;

        /// <summary>
        /// The default number of days for the media cache control header.
        /// </summary>
        public int MaxBrowserCacheDays { get; set; } = 30;

        /// <summary>
        /// The maximum number of days a cached resized media item will be valid for, before being rebuilt on request.
        /// </summary>
        public int MaxCacheDays { get; set; } = 365;

        /// <summary>
        /// The maximum size of an uploaded file in bytes. 
        /// NB: You might still need to configure the limit in IIS (https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/requestfiltering/requestlimits/)
        /// </summary>
        public int MaxFileSize { get; set; } = 30_000_000;

        /// <summary>
        /// A Cdn base url which will be prefixed to all media paths.
        /// </summary>
        public string CdnBaseUrl { get; set; }

        // Setting a default value here will make the IShellConfiguration add to the default values, rather than replace.

        /// <summary>
        /// The list of allowed file extensions.
        /// </summary>
        public string[] AllowedFileExtensions { get; set; }

        /// <summary>
        /// The accepted sizes for custom width and height.
        /// </summary>
        public int[] SupportedSizes { get; set; }

    }
}
