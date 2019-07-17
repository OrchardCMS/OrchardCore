using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media
{
    public class MediaOptions
    {
        /// <summary>
        /// The request path used to route asset files
        /// </summary>
        public static readonly PathString AssetsRequestPath = new PathString("/media");

        public int MaxBrowserCacheDays { get; set; } = 30;
        public int MaxCacheDays { get; set; } = 365;

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

        private int[] _supportedSizes = new[] { 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048 };
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
    }
}
