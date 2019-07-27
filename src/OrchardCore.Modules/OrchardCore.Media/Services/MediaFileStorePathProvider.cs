using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

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
            _requestBaseUrl = "/" + IMediaFileStorePathProviderHelpers.Combine(shellSettings.RequestUrlPrefix, mediaOptions.Value.AssetsRequestPath);

            var originalPathBase = httpContextAccessor
                .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

            if (originalPathBase.HasValue)
            {
                _requestBaseUrl = IMediaFileStorePathProviderHelpers.Combine(originalPathBase, _requestBaseUrl);
            }

            _cdnBaseUrl = mediaOptions.Value.CdnBaseUrl;
        }

        public string MapPathToPublicUrl(string path)
        {
            //TODO fix extensions
            return _cdnBaseUrl + _requestBaseUrl.TrimEnd('/') + "/" + IMediaFileStorePathProviderHelpers.NormalizePath(path);
        }
        public string MapRequestPathToFileStorePath(string requestPath)
        {
            //Hmm maybe bring back what I wrote for the other one

            //if (publicUrl.StartsWith(_cdnBaseUrl))
            //{
            //    var resolvedPath = publicUrl.Substring(_cdnBaseUrl.Length);
            //    if (resolvedPath.StartsWith(_publicUrlBase))
            //    {
            //        return resolvedPath.Substring(_publicUrlBase.Length);
            //    }
            //    else
            //    {
            //        return resolvedPath;
            //    }
            //}
            //else if (publicUrl.StartsWith(_publicUrlBase, StringComparison.OrdinalIgnoreCase))
            //{
            //    return publicUrl.Substring(_publicUrlBase.Length);
            //}
            //else
            //{
            //    throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            //}


            //TODO make this MapSiteUrlToPath
            if (!requestPath.StartsWith(_requestBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(requestPath), "The specified URL is not inside the URL scope of the file store.");
            }

            return requestPath.Substring(_requestBaseUrl.Length);
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
