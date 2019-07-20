using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Azure.Services
{
    //TODO This currently maintains the original blob configuration, so allows serving direct form blob, but no resizing
    public class MediaBlobFileStorePathProvider : IMediaFileStorePathProvider
    {
        private readonly string _siteUrlBase;
        private readonly string _publicUrlBase;
        public MediaBlobFileStorePathProvider(
            IOptions<MediaBlobStorageOptions> mediaBlobStorageOptions,
            IOptions<MediaOptions> mediaOptions,
            IHttpContextAccessor httpContextAccessor,
            ShellSettings shellSettings
            )
        {
            // These do not make http calls, or verify that connection is valid, and blob can connect
            //We should use TryParse in Filter. Check whether this throws on a bad ConnectionString
            var storageAccount = CloudStorageAccount.Parse(mediaBlobStorageOptions.Value.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(mediaBlobStorageOptions.Value.ContainerName);

            var uriBuilder = new UriBuilder(blobContainer.Uri);
            uriBuilder.Path = IMediaFileStorePathProviderHelpers.Combine(uriBuilder.Path, mediaBlobStorageOptions.Value.BasePath);
            var mediaBaseUri = uriBuilder.Uri;

            if (!String.IsNullOrEmpty(mediaBlobStorageOptions.Value.PublicHostName))
                mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = mediaBlobStorageOptions.Value.PublicHostName }.Uri;

            _publicUrlBase = mediaBaseUri.ToString();

            _siteUrlBase = "/" + IMediaFileStorePathProviderHelpers.Combine(shellSettings.RequestUrlPrefix, mediaOptions.Value.AssetsRequestPath);

            var originalPathBase = httpContextAccessor
                .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

            if (originalPathBase.HasValue)
            {
                _siteUrlBase = IMediaFileStorePathProviderHelpers.Combine(originalPathBase, _siteUrlBase);
            }
        }

        public string MapPathToPublicUrl(string path)
        {
            //TODO fix extensions
            return _publicUrlBase.TrimEnd('/') + "/" + IMediaFileStorePathProviderHelpers.NormalizePath(path);
        }
        /// <summary>
        /// This will map a public url, such as http://127.0.0.1:10000/devstoreaccount1/somecontainer/blobmedia/an-image.png
        /// to the standard virtual media path, e.g. /media/an-image.png
        /// </summary>
        /// <param name="publicUrl"></param>
        /// <returns></returns>
        public string MapPublicUrlToPath(string publicUrl)
        {

            //TODO make this MapSiteUrlToPath ??
            if (!publicUrl.StartsWith(_siteUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            }

            return publicUrl.Substring(_siteUrlBase.Length);
        }

        public bool MatchCdnPath(string path)
        {
            return !String.IsNullOrEmpty(_publicUrlBase) && path.StartsWith(_publicUrlBase);
        }

        /// <summary>
        /// This will remove the public url from the path, and convert the path to include /media
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string RemoveCdnPath(string path)
        {
            //return here needs to be /media/catbot

            //TODO make this MapSiteUrlToPath ??
            if (!path.StartsWith(_publicUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(path), "The specified URL is not inside the URL scope of the file store.");
            }

            return _siteUrlBase + path.Substring(_publicUrlBase.Length);
        }
    }
}