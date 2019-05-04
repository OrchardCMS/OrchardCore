using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
//using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;


namespace OrchardCore.ResourceManagement
{
    /// <summary>
    /// Provides version hash for a specified file.
    /// </summary>
    public class FileVersionProvider : IFileVersionProvider
    {
        private const string VersionKey = "v";
        private static readonly char[] QueryStringAndFragmentTokens = new[] { '?', '#' };

        private readonly IEnumerable<IFileProvider> _registeredFileProviders;
        private readonly IFileProvider _webRootfileProvider;

        private IList<IFileProvider> _combinedFileProviders;
        //private readonly IMemoryCache _tenantCache;
        public FileVersionProvider(
            IEnumerable<IFileProvider> registeredFileProviders,
            IHostingEnvironment environment
            //,
            //IMemoryCache tenantCache
            )
        {
            _registeredFileProviders = registeredFileProviders;
            _webRootfileProvider = environment.WebRootFileProvider;
            //_tagHelperCache = cacheProvider.Cache;
            //_tenantCache = tenantCache;
        }



        public string AddFileVersionToPath(PathString requestPathBase, string path)
        {
            //build _combinedFileProviders as required
            //we could register env.WebRoot as well, but this allows someone to mutate env.WebRoot 
            if (_combinedFileProviders == null)
            {
                _combinedFileProviders = _registeredFileProviders.ToList();
                if (_webRootfileProvider != null)
                {
                    _combinedFileProviders.Add(_webRootfileProvider);
                }
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var resolvedPath = path;

            var queryStringOrFragmentStartIndex = path.IndexOfAny(QueryStringAndFragmentTokens);
            if (queryStringOrFragmentStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringOrFragmentStartIndex);
            }
            resolvedPath = resolvedPath.TrimStart('~');

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                // Don't append version if the path is absolute.
                return path;
            }

            //if (Cache.TryGetValue(path, out string value))
            //{
            //    return value;
            //}
            string value = path;
            //var cacheEntryOptions = new MemoryCacheEntryOptions();
            //cacheEntryOptions.AddExpirationToken(FileProvider.Watch(resolvedPath));
            foreach (var fileProvider in _registeredFileProviders)
            {
                var fileInfo = fileProvider.GetFileInfo(resolvedPath);

                if (!fileInfo.Exists &&
                    requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var requestPathBaseRelativePath = resolvedPath.Substring(requestPathBase.Value.Length);
                    //cacheEntryOptions.AddExpirationToken(fileProvider.Watch(requestPathBaseRelativePath));
                    fileInfo = fileProvider.GetFileInfo(requestPathBaseRelativePath);
                }

                if (fileInfo.Exists)
                {
                    value = QueryHelpers.AddQueryString(path, VersionKey, GetHashForFile(fileInfo));
                }
                break;
               

                //cacheEntryOptions.SetSize(value.Length * sizeof(char));
                //value = _tagHelperCache.Set(path, value, cacheEntryOptions);
                //return value;
            }

            //else
            //{
            //    // if the file is not in the current server.
            //    value = path;
            //}
            return value;
        }

        private static string GetHashForFile(IFileInfo fileInfo)
        {
            using (var sha256 = CryptographyAlgorithms.CreateSHA256())
            {
                using (var readStream = fileInfo.CreateReadStream())
                {
                    var hash = sha256.ComputeHash(readStream);
                    return WebEncoders.Base64UrlEncode(hash);
                }
            }
        }
    }
}