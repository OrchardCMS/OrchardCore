using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Processing;

public class MediaTokenService : IMediaTokenService
{
    private const string TokenCacheKeyPrefix = "MediaToken:";

    private static readonly HashSet<string> _knownCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        MediaCommands.WidthCommand,
        MediaCommands.HeightCommand,
        MediaCommands.ResizeModeCommand,
        MediaCommands.ResizeFocalPointCommand,
        MediaCommands.FormatCommand,
        MediaCommands.BackgroundColorCommand,
        MediaCommands.QualityCommand,
        MediaCommands.AutoOrientCommand,
        MediaCommands.TokenCommand,
        MediaCommands.VersionCommand,
    };

    private readonly IMemoryCache _memoryCache;
    private readonly byte[] _hashKey;

    public MediaTokenService(
        IMemoryCache memoryCache,
        IOptions<MediaTokenOptions> options)
    {
        _memoryCache = memoryCache;
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

        // If no commands or only a version command, don't tokenize.
        if (processingCommands is null
            || processingCommands.Count == 0
            || (processingCommands.Count == 1 && processingCommands.ContainsKey(MediaCommands.VersionCommand)))
        {
            return path;
        }

        var queryStringTokenKey = CreateTokenKey(processingCommands.Select(p => KeyValuePair.Create(p.Key, p.Value.ToString())));
        var token = GetHash(queryStringTokenKey);
        processingCommands[MediaCommands.TokenCommand] = token;

        if (otherCommands != null)
        {
            foreach (var command in otherCommands)
            {
                processingCommands.Add(command.Key, command.Value);
            }
        }

        return AddQueryString(path.AsSpan(0, pathIndex), processingCommands);
    }

    public bool TryValidateToken(IEnumerable<KeyValuePair<string, string>> commands, string token)
    {
        var queryStringTokenKey = CreateTokenKey(commands);
        return string.Equals(GetHash(queryStringTokenKey), token, StringComparison.OrdinalIgnoreCase);
    }

    private static void ParseQuery(
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

    private static string CreateTokenKey(IEnumerable<KeyValuePair<string, string>> values)
    {
        using var builder = ZString.CreateStringBuilder();
        builder.Append(TokenCacheKeyPrefix);
        foreach (var pair in values)
        {
            builder.Append(pair.Value);
        }

        return builder.ToString();
    }

    private string GetHash(string tokenKey)
    {
        if (!_memoryCache.TryGetValue(tokenKey, out var result))
        {
            using var entry = _memoryCache.CreateEntry(tokenKey);
            entry.SlidingExpiration = TimeSpan.FromHours(5);

            var chars = tokenKey.AsSpan(TokenCacheKeyPrefix.Length);
            var requiredLength = Encoding.UTF8.GetByteCount(chars);
            var stringBytes = requiredLength < 1024
                ? stackalloc byte[requiredLength]
                : new byte[requiredLength];

            Span<byte> hashBytes = stackalloc byte[HMACSHA256.HashSizeInBytes];
            var stringBytesLength = Encoding.UTF8.GetBytes(chars, stringBytes);
            HMACSHA256.TryHashData(_hashKey, stringBytes[..stringBytesLength], hashBytes, out var hashBytesLength);

            entry.Value = result = Convert.ToBase64String(hashBytes[..hashBytesLength]);
        }

        return (string)result;
    }

    private static string AddQueryString(
        ReadOnlySpan<char> uri,
        Dictionary<string, StringValues> queryString)
    {
        var anchorIndex = uri.IndexOf('#');
        var uriToBeAppended = uri;
        var anchorText = ReadOnlySpan<char>.Empty;

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
