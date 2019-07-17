using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Media.Services
{
    public class MediaFileStorePathProvider : IMediaFileStorePathProvider
    {
        private readonly string _cdnBaseUrl;
        private readonly string _siteUrlBase;
        public MediaFileStorePathProvider(string siteUrlBase, string cdnBaseUrl)
        {
            _cdnBaseUrl = cdnBaseUrl;
            _siteUrlBase = siteUrlBase;
        }

        public string MapPathToPublicUrl(string path)
        {
            //TODO fix extensions
            return _cdnBaseUrl + _siteUrlBase.TrimEnd('/') + "/" + IMediaFileStorePathProviderHelpers.NormalizePath(path);
        }
        public string MapPublicUrlToPath(string publicUrl)
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
            if (!publicUrl.StartsWith(_siteUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            }

            return publicUrl.Substring(_siteUrlBase.Length);
        }

        public bool MatchCdnPath(string path)
        {
            return !String.IsNullOrEmpty(_cdnBaseUrl) && path.StartsWith(_cdnBaseUrl);
        }

        public string RemoveCdnPath(string path)
        {
            return path.Substring(_cdnBaseUrl.Length);
        }
    }
}
