using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media
{
    public class MediaOptions
    {
        /// <summary>
        /// The accepted sizes for custom width and height.
        /// </summary>
        // Setting a default value will make the IShellConfiguration add to the default values, rather than replace.
        public int[] SupportedSizes { get; set; }

        /// <summary>
        /// The list of allowed file extensions.
        /// </summary>
        // Setting a default value will make the IShellConfiguration add to the default values, rather than replace.
        public HashSet<string> AllowedFileExtensions { get; set; }

        /// <summary>
        /// The default number of days for the media cache control header.
        /// </summary>
        public int MaxBrowserCacheDays { get; set; }

        /// <summary>
        /// The maximum number of days a cached resized media item will be valid for, before being rebuilt on request.
        /// </summary>
        public int MaxCacheDays { get; set; }

        /// <summary>
        /// The maximum size of an uploaded file in bytes.
        /// NB: You might still need to configure the limit in IIS (https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/requestfiltering/requestlimits/)
        /// </summary>
        public int MaxFileSize { get; set; }

        /// <summary>
        /// A CDN base url that will be prefixed to the request path when serving images.
        /// </summary>
        public string CdnBaseUrl { get; set; }

        /// <summary>
        /// The request path used to route asset files.
        /// </summary>
        public PathString AssetsRequestPath { get; set; }

        /// <summary>
        /// The path used to store media assets. The path can be relative to the tenant's App_Data folder, or absolute.
        /// </summary>
        public string AssetsPath { get; set; }
    }
}
