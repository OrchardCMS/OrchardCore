using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Azure
{
    //TODO This currently maintains the original blob configuration, so allows serving direct form blob, but no resizing
    public class MediaBlobFileStorePathProvider : IMediaFileStorePathProvider
    {
        private readonly string _publicUrlBase;
        public MediaBlobFileStorePathProvider(string publicUrlBase)
        {
            _publicUrlBase = publicUrlBase;
        }

        public string MapPathToPublicUrl(string path)
        {
            //TODO fix extensions
            return _publicUrlBase.TrimEnd('/') + "/" + IMediaFileStorePathProviderHelpers.NormalizePath(path);
        }
        public string MapPublicUrlToPath(string publicUrl)
        {

            //TODO make this MapSiteUrlToPath ??
            if (!publicUrl.StartsWith(_publicUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            }

            return publicUrl.Substring(_publicUrlBase.Length);
        }

        public bool MatchCdnPath(string path)
        {
            return false;
        }

        public string RemoveCdnPath(string path)
        {
            return path;
        }
    }
}