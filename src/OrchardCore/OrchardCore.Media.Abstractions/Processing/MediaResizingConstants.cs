namespace OrchardCore.Media;

/// <summary>
/// Shared constants and helpers describing the image formats supported by on-demand media
/// resizing, kept in one place so the processing engine, middleware and resized-image caches
/// agree on content types and file extensions.
/// </summary>
public static class MediaResizingConstants
{
    public const string JpegContentType = "image/jpeg";
    public const string PngContentType = "image/png";
    public const string WebpContentType = "image/webp";
    public const string GifContentType = "image/gif";

    /// <summary>
    /// Maps a resized-image content type to the file extension (including the leading dot) used
    /// when caching the resized image. Unknown content types fall back to JPEG.
    /// </summary>
    public static string ContentTypeToExtension(string contentType) => contentType switch
    {
        PngContentType => ".png",
        WebpContentType => ".webp",
        GifContentType => ".gif",
        _ => ".jpg",
    };

    /// <summary>
    /// Maps a cached resized-image file extension (including the leading dot) to its content type.
    /// Unknown extensions fall back to JPEG.
    /// </summary>
    public static string ExtensionToContentType(string extension) => extension switch
    {
        ".png" => PngContentType,
        ".webp" => WebpContentType,
        ".gif" => GifContentType,
        _ => JpegContentType,
    };
}
