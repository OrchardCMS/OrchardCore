using System;
using System.Linq;
using OrchardCore.FileStorage;

namespace OrchardCore.Media
{
    public interface IMediaFileStorePathProvider : ICdnPathProvider
    {
        /// <summary>
        /// Maps a path within the file store to a publicly accessible URL.
        /// </summary>
        /// <param name="path">The path within the file store.</param>
        /// <returns>A string containing the mapped public URL of the given path.</returns>
        string MapPathToPublicUrl(string path);

        /// <summary>
        /// Maps a public URL to a path within the file store.
        /// </summary>
        /// <param name="publicUrl">The public URL to map.</param>
        /// <returns>The mapped path of the given public URL within the file store.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the specified <paramref name="publicUrl"/> value is not within the publicly accessible URL space of the file store.</exception>
        string MapPublicUrlToPath(string publicUrl);
    }

    //TODO rationalise these extensions together
    public static class IMediaFileStorePathProviderHelpers
    {
        /// <summary>
        /// Combines multiple path parts using the path delimiter semantics of the abstract virtual file store. 
        /// </summary>
        /// <param name="paths">The path parts to combine.</param>
        /// <returns>The full combined path.</returns>
        public static string Combine(params string[] paths)
        {
            if (paths.Length == 0)
                return null;

            var normalizedParts =
                paths
                    .Select(x => NormalizePath(x))
                    .Where(x => !String.IsNullOrEmpty(x))
                    .ToArray();

            var combined = String.Join("/", normalizedParts);

            // Preserve the initial '/' if it's present.
            if (paths[0]?.StartsWith("/") == true)
                combined = "/" + combined;

            return combined;
        }

        /// <summary>
        /// Normalizes a path using the path delimiter semantics of the abstract virtual file store.
        /// </summary>
        /// <remarks>
        /// Backslash is converted to forward slash and any leading or trailing slashes
        /// are removed.
        /// </remarks>
        public static string NormalizePath(string path)
        {
            if (path == null)
                return null;

            return path.Replace('\\', '/').Trim('/');
        }
    }
}
