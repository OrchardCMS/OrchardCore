namespace OrchardCore.Media.Services;

internal static class MediaFileStorePathHelper
{
    public static bool IsValidRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path[0] is '/' or '\\' ||
            path.Contains(':') ||
            path.Contains('\0'))
        {
            return false;
        }

        // Apply the same rules on every platform, including Windows path aliases.
        return path.Split(PathExtensions.PathSeparators).All(segment =>
            !string.IsNullOrWhiteSpace(segment) &&
            !segment.EndsWith('.') &&
            !segment.EndsWith(' '));
    }

    /// <summary>
    /// Checks that a canonical physical path is strictly contained in a canonical directory path.
    /// </summary>
    /// <remarks>
    /// An ordinal comparison is used on every platform, see <see cref="Modules.FileProviders.PhysicalPathResolver"/> for why the
    /// operating system is not a reliable proxy for the case sensitivity of the file system.
    /// </remarks>
    public static bool IsWithinDirectory(string directoryPath, string physicalPath)
    {
        var directoryPrefix = Path.EndsInDirectorySeparator(directoryPath)
            ? directoryPath
            : directoryPath + Path.DirectorySeparatorChar;

        return physicalPath.StartsWith(directoryPrefix, StringComparison.Ordinal);
    }
}
