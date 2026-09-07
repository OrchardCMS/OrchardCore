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
}
