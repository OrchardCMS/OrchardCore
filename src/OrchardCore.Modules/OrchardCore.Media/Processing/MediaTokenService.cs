using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenService : IMediaTokenService
    {
        private const string TokenCacheKeyPrefix = "MediaToken:";
        private readonly IMemoryCache _memoryCache;

        private readonly HashSet<string> _knownCommands = new HashSet<string>();
        private readonly byte[] _hashKey;

        public MediaTokenService(
            IMemoryCache memoryCache,
            IOptions<MediaTokenOptions> options,
            IEnumerable<IImageWebProcessor> processors)
        {
            _memoryCache = memoryCache;
            foreach (var processor in processors)
            {
                foreach (var command in processor.Commands)
                {
                    _knownCommands.Add(command);
                }
            }

            _hashKey = options.Value.HashKey;
        }

        public string AddTokenToPath(string path)
        {
            var pathParts = path.Split('?');

            var parsed = QueryHelpers.ParseQuery(pathParts.Length > 1 ? pathParts[1] : string.Empty);

            // If no commands or only a version command don't bother tokenizing.
            if (parsed.Count == 0 || parsed.Count == 1 && parsed.ContainsKey(ImageVersionProcessor.VersionCommand))
            {
                return path;
            }

            var processingCommands = new Dictionary<string, string>();
            Dictionary<string, string> otherCommands = null;

            foreach (var command in parsed)
            {
                if (_knownCommands.Contains(command.Key))
                {
                    processingCommands[command.Key] = command.Value.ToString();
                }
                else
                {
                    otherCommands ??= new Dictionary<string, string>();
                    otherCommands[command.Key] = command.Value.ToString();
                }
            }

            // Using the command values as a key retrieve from cache
            var commandValues = String.Concat(processingCommands.Values);

            var queryStringTokenKey = TokenCacheKeyPrefix + commandValues;

            var queryStringToken = GetHash(commandValues, queryStringTokenKey);

            processingCommands["token"] = queryStringToken;

            // If any non-resizing paramters have been added to the path include these on the query string output.
            if (otherCommands != null)
            {
                foreach (var command in otherCommands)
                {
                    processingCommands.Add(command.Key, command.Value);
                }
            }

            return QueryHelpers.AddQueryString(pathParts[0], processingCommands);
        }

        public bool TryValidateToken(IDictionary<string, string> commands, string token)
        {
            var commandValues = String.Concat(commands.Values);

            var queryStringTokenKey = TokenCacheKeyPrefix + commandValues;

            // Store a hash of the valid query string commands.
            var queryStringToken = GetHash(commandValues, queryStringTokenKey);

            if (String.Equals(queryStringToken, token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private string GetHash(string commandValues, string queryStringTokenKey)
            => _memoryCache.GetOrCreate(queryStringTokenKey, entry =>
               {
                   entry.SlidingExpiration = TimeSpan.FromHours(5);

                   using var hmac = new HMACSHA256(_hashKey);

                   var bytes = Encoding.UTF8.GetBytes(commandValues);

                   var hash = hmac.ComputeHash(bytes);

                   return Convert.ToBase64String(hash);
               });
    }
}
