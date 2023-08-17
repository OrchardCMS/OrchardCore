using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenService : IMediaTokenService
    {
        private const string TokenCacheKeyPrefix = "MediaToken:";
        private readonly IMemoryCache _memoryCache;

        private readonly HashSet<string> _knownCommands = new(12);
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
            var pathIndex = path.IndexOf('?');

            Dictionary<string, StringValues> processingCommands = null;
            Dictionary<string, StringValues> otherCommands = null;

            if (pathIndex != -1)
            {
                ParseQuery(path[(pathIndex + 1)..], out processingCommands, out otherCommands);
            }

            // If no commands or only a version command don't bother tokenizing.
            if (processingCommands is null
                || processingCommands.Count == 0
                || processingCommands.Count == 1 && processingCommands.ContainsKey(ImageVersionProcessor.VersionCommand))
            {
                return path;
            }

            // Using the command values as a key retrieve from cache.
            var queryStringTokenKey = CreateQueryStringTokenKey(processingCommands);
            var queryStringToken = GetHash(queryStringTokenKey);

            processingCommands["token"] = queryStringToken;

            // If any non-resizing parameters have been added to the path include these on the query string output.
            if (otherCommands != null)
            {
                foreach (var command in otherCommands)
                {
                    processingCommands.Add(command.Key, command.Value);
                }
            }

            return AddQueryString(path.AsSpan(0, pathIndex), processingCommands);
        }

        private void ParseQuery(
            string queryString,
            out Dictionary<string, StringValues> processingCommands,
            out Dictionary<string, StringValues> otherCommands)
        {
            processingCommands = null;
            otherCommands = null;

            var accumulator = new KeyValueAccumulator();
            var otherCommandAccumulator = new KeyValueAccumulator();
            var enumerable = new QueryStringEnumerable(queryString);

            foreach (var pair in enumerable)
            {
                var key = pair.DecodeName().ToString();
                var value = pair.DecodeValue().ToString();

                if (_knownCommands.Contains(key))
                {
                    accumulator.Append(key, value);
                }
                else
                {
                    otherCommandAccumulator.Append(key, value);
                }
            }

            if (accumulator.HasValues)
            {
                processingCommands = accumulator.GetResults();
            }

            if (otherCommandAccumulator.HasValues)
            {
                otherCommands = otherCommandAccumulator.GetResults();
            }
        }

        public bool TryValidateToken(KeyedCollection<string, KeyValuePair<string, string>> commands, string token)
        {
            var queryStringTokenKey = CreateCommandCollectionTokenKey(commands);

            // Store a hash of the valid query string commands.
            var queryStringToken = GetHash(queryStringTokenKey);

            if (String.Equals(queryStringToken, token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Specialized version with fast enumeration and no StringValues string allocation.
        /// </summary>
        private static string CreateQueryStringTokenKey(Dictionary<string, StringValues> values)
        {
            using var builder = ZString.CreateStringBuilder();
            builder.Append(TokenCacheKeyPrefix);
            foreach (var pair in values)
            {
                builder.Append(pair.Value.ToString());
            }

            return builder.ToString();
        }

        private static string CreateCommandCollectionTokenKey(KeyedCollection<string, KeyValuePair<string, string>> values)
        {
            using var builder = ZString.CreateStringBuilder();
            builder.Append(TokenCacheKeyPrefix);
            foreach (var pair in values)
            {
                builder.Append(pair.Value);
            }

            return builder.ToString();
        }

        private string GetHash(string queryStringTokenKey)
        {
            if (!_memoryCache.TryGetValue(queryStringTokenKey, out var result))
            {
                using var entry = _memoryCache.CreateEntry(queryStringTokenKey);

                entry.SlidingExpiration = TimeSpan.FromHours(5);

                using var hmac = new HMACSHA256(_hashKey);

                // 'queryStringTokenKey' also contains prefix.
                var chars = queryStringTokenKey.AsSpan(TokenCacheKeyPrefix.Length);

                // Only allocate on stack if it's small enough.
                var requiredLength = Encoding.UTF8.GetByteCount(chars);
                var stringBytes = requiredLength < 1024
                    ? stackalloc byte[requiredLength]
                    : new byte[requiredLength];

                // 256 for SHA-256, fits in stack nicely.
                Span<byte> hashBytes = stackalloc byte[hmac.HashSize];

                var stringBytesLength = Encoding.UTF8.GetBytes(chars, stringBytes);

                hmac.TryComputeHash(stringBytes[..stringBytesLength], hashBytes, out var hashBytesLength);

                entry.Value = result = Convert.ToBase64String(hashBytes[..hashBytesLength]);
            }

            return (string)result;
        }

        /// <summary>
        /// Custom version of <see cref="QueryHelpers.AddQueryString(String,String,String)"/> that takes our pre-built
        /// dictionary, uri as ReadOnlySpan&lt;char&gt; and uses ZString. Otherwise same logic.
        /// </summary>
        private static string AddQueryString(
            ReadOnlySpan<char> uri,
            Dictionary<string, StringValues> queryString)
        {
            var anchorIndex = uri.IndexOf('#');
            var uriToBeAppended = uri;
            var anchorText = ReadOnlySpan<char>.Empty;

            // If there is an anchor, then the query string must be inserted before its first occurrence.
            if (anchorIndex != -1)
            {
                anchorText = uri[anchorIndex..];
                uriToBeAppended = uri[..anchorIndex];
            }

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            using var sb = ZString.CreateStringBuilder();
            sb.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            sb.Append(anchorText);

            return sb.ToString();
        }
    }
}
