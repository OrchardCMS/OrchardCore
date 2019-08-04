using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media
{
    public class MediaOptions
    {
        private static readonly int[] DefaultSupportedSizes = new[] { 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048 };

        private static readonly string[] DefaultAllowedFileExtensions = new string[] {
            // Images
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".ico",
            ".svg",

            // Documents
            ".pdf", // (Portable Document Format; Adobe Acrobat)
            ".doc", ".docx", // (Microsoft Word Document)
            ".ppt", ".pptx", ".pps", ".ppsx", // (Microsoft PowerPoint Presentation)
            ".odt", // (OpenDocument Text Document)
            ".xls", ".xlsx", // (Microsoft Excel Document)
            ".psd", // (Adobe Photoshop Document)

            // Audio
            ".mp3",
            ".m4a",
            ".ogg",
            ".wav",

            // Video
            ".mp4", ".m4v", // (MPEG-4)
            ".mov", // (QuickTime)
            ".wmv", // (Windows Media Video)
            ".avi",
            ".mpg",
            ".ogv", // (Ogg)
            ".3gp", // (3GPP)
        };

        /// <summary>
        /// The request path used to route asset files
        /// </summary>
        public PathString AssetsRequestPath = new PathString("/media");

        /// <summary>
        /// Specify the type of cache to use when resizing media.
        /// </summary>

        public CacheConfiguration CacheConfiguration = CacheConfiguration.Physical;

        public string AssetsCachePath = "MediaCache";
        public int MaxBrowserCacheDays { get; set; } = 30;
        public int MaxCacheDays { get; set; } = 365;
        public int MaxFileSize { get; set; } = 30_000_000;

        private string _cdnBaseUrl;
        public string CdnBaseUrl
        {
            get
            {
                return _cdnBaseUrl;
            }
            set
            {
                _cdnBaseUrl = value != null ? value.TrimEnd('/').ToLower() : String.Empty;
            }
        }

        private int[] _supportedSizes = DefaultSupportedSizes;
        public int[] SupportedSizes
        {
            get
            {
                return _supportedSizes;
            }
            set
            {
                if (value != null)
                {
                    _supportedSizes = value.OrderBy(s => s).ToArray();
                }
            }
        }

        private string[] _allowedFileExtensions = DefaultAllowedFileExtensions;
        public string[] AllowedFileExtensions
        {
            get
            {
                return _allowedFileExtensions;
            }
            set
            {
                if (value != null)
                {
                    _allowedFileExtensions = value;
                }
            }
        }
    }
}
