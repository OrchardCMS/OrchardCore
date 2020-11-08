using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OrchardCore.Modules;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenService : IMediaTokenService
    {
        private readonly IDataProtector _dataProtector;
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _knownCommands = new HashSet<string>();

        private const string TokenCacheKeyPrefix = "MediaToken:";
        private const string CommandCacheKeyPrefix = "MediaCommands:";

        public MediaTokenService(
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache memoryCache,
            IEnumerable<IImageWebProcessor> processors)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("MediaProcessingTokenService");
            _memoryCache = memoryCache;
            foreach (IImageWebProcessor processor in processors)
            {
                foreach (string command in processor.Commands)
                {
                    _knownCommands.Add(command);
                }
            }
        }

        public string TokenizePath(string path)
        {
            var pathParts = path.Split('?');

            var parsed = QueryHelpers.ParseQuery(pathParts.Length > 1 ? pathParts[1] : string.Empty);
            var processingCommands = new Dictionary<string, string>();
            var otherCommands = new Dictionary<string, string>();

            foreach (var command in parsed)
            {
                if (_knownCommands.Contains(command.Key))
                {
                    processingCommands[command.Key] = command.Value.ToString();
                }
                else
                {
                    otherCommands[command.Key] = command.Value.ToString();
                }
            }

            // Using the command values as a key retrieve from cache
            var key = String.Concat(processingCommands.Values);

            var queryStringTokenKey = TokenCacheKeyPrefix + key;

            var queryStringToken = _memoryCache.GetOrCreate(queryStringTokenKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(5);

                var json = JsonConvert.SerializeObject(processingCommands);

                return _dataProtector.Protect(json);
            });

            otherCommands["token"] = queryStringToken;

            var commandCacheKey = CommandCacheKeyPrefix + key;

            var query = _memoryCache.GetOrCreate(commandCacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(24);
                return processingCommands;
            });

            return QueryHelpers.AddQueryString(pathParts[0], otherCommands);
        }

        public IDictionary<string, string> GetTokenizedCommands(string token)
        {
            return _memoryCache.GetOrCreate(token, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(24);
                try
                {
                    var json = _dataProtector.Unprotect(token);

                    return JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
                }
                catch
                {
                }

                return null;
            });
        }
    }
}
