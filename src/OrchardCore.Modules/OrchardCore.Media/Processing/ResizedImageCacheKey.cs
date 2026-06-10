#nullable enable

using System.Security.Cryptography;
using System.Text;

namespace OrchardCore.Media.Processing;

/// <summary>
/// Computes the storage-agnostic cache key for a resized image. Kept independent of any
/// <see cref="IResizedImageCache"/> implementation so the request pipeline does not depend on a
/// concrete cache type.
/// </summary>
internal static class ResizedImageCacheKey
{
    public static string Compute(string tenantName, string path, IEnumerable<KeyValuePair<string, string>> commands)
    {
        var builder = new StringBuilder();
        builder.Append(tenantName).Append('|').Append(path).Append('|');

        foreach (var (key, value) in commands.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(key).Append('=').Append(value).Append('&');
        }

        var raw = Encoding.UTF8.GetBytes(builder.ToString());
        var hash = SHA256.HashData(raw);

        return Convert.ToHexStringLower(hash);
    }
}
