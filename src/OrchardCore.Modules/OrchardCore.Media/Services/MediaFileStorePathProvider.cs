using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Services
{
    public class MediaFileStorePathProvider : IMediaFileStorePathProvider
    {
        private readonly string _requestBaseUrl;
        private readonly string _cdnBaseUrl;

        public MediaFileStorePathProvider(
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MediaOptions> mediaOptions
            )
        {
            _requestBaseUrl = "/" + IFileStoreExtensions.Combine(null, shellSettings.RequestUrlPrefix, mediaOptions.Value.AssetsRequestPath);

            var originalPathBase = httpContextAccessor
                .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

            if (originalPathBase.HasValue)
            {
                _requestBaseUrl = IFileStoreExtensions.Combine(null, originalPathBase, _requestBaseUrl);
            }

            _requestBaseUrl = _requestBaseUrl.TrimEnd('/');

            // Media options configuration ensures any trailing slash is removed.
            _cdnBaseUrl = mediaOptions.Value.CdnBaseUrl;
        }

        public string MapPathToPublicUrl(string path)
        {
            return _cdnBaseUrl + _requestBaseUrl + "/" + IFileStoreExtensions.NormalizePath(null, path);
        }

        public string MapPublicUrlToPath(string publicUrl)
        {
            // Services typically remove the cdn url before reaching this mapping, but double check here. 
            if (publicUrl.StartsWith(_cdnBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                publicUrl = publicUrl.Substring(_cdnBaseUrl.Length);
                if (publicUrl.StartsWith(_requestBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return publicUrl.Substring(_requestBaseUrl.Length);
                }
                else
                {
                    return publicUrl;
                }
            }
            else if (publicUrl.StartsWith(_requestBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return publicUrl.Substring(_requestBaseUrl.Length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            }
        }

        public bool MatchCdnPath(string path)
        {
            return !String.IsNullOrEmpty(_cdnBaseUrl) && path.StartsWith(_cdnBaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        public string RemoveCdnPath(string path)
        {
            return path.Substring(_cdnBaseUrl.Length);
        }
    }
}
