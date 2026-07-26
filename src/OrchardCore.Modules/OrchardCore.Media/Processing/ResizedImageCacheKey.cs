#nullable enable

using System.Security.Cryptography;
using Cysharp.Text;

namespace OrchardCore.Media.Processing;

/// <summary>
/// Computes the storage-agnostic cache key for a resized image. Kept independent of any
/// <see cref="IResizedImageCache"/> implementation so the request pipeline does not depend on a
/// concrete cache type.
/// </summary>
/// <remarks>
/// The returned value is used as a durable storage identifier (the cache file name, blob name or
/// object key, with its first characters used as a shard directory), so it must be a stable,
/// bounded-length and filesystem-safe string rather than an in-memory composite key. Hashing also
/// keeps the key deterministic across processes, unlike a runtime <c>GetHashCode</c>.
/// </remarks>
internal static class ResizedImageCacheKey
{
    private static readonly Comparison<KeyValuePair<string, string>> _byOrdinalKey =
        static (a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);

    public static string Compute(string tenantName, string path, IEnumerable<KeyValuePair<string, string>> commands)
    {
        // Sort the (few) commands deterministically. Materializing into a small array and sorting in
        // place avoids the extra allocations of LINQ's OrderBy.
        var sorted = commands.ToArray();
        Array.Sort(sorted, _byOrdinalKey);

        // Build the payload directly into a pooled UTF-8 buffer and hash that span, so no
        // intermediate System.String or managed byte[] is allocated for the key material.
        using var builder = ZString.CreateUtf8StringBuilder();
        builder.Append(tenantName);
        builder.Append('|');
        builder.Append(path);
        builder.Append('|');

        foreach (var (key, value) in sorted)
        {
            builder.Append(key);
            builder.Append('=');
            builder.Append(value);
            builder.Append('&');
        }

        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(builder.AsSpan(), hash);

        return Convert.ToHexStringLower(hash);
    }
}
