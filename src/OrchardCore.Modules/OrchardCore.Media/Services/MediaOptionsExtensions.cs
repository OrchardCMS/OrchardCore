namespace OrchardCore.Media;

internal static class MediaOptionsExtensions
{
    public static bool IsFileExtensionAllowed(
        this MediaOptions options,
        string extension,
        bool hasAdditionalPermission)
        => options.AllowedFileExtensions.Contains(extension)
            || (hasAdditionalPermission && options.AllowedFileExtensionsWithPermission.Contains(extension));
}
