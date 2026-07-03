using OrchardCore.Media.Core.Processing;

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
    /// Parses a requested format name (for example <c>png</c> or <c>webp</c>) into a
    /// <see cref="Format"/>. Unknown or unsupported names return <see cref="Format.Undefined"/>,
    /// which engines treat as the default JPEG output.
    /// </summary>
    public static Format ParseFormat(string format) => format?.TrimStart('.').ToLowerInvariant() switch
    {
        "png" => Format.Png,
        "gif" => Format.Gif,
        "webp" => Format.WebP,
        "jpg" or "jpeg" => Format.Jpg,
        _ => Format.Undefined,
    };

    /// <summary>
    /// Maps an output <see cref="Format"/> to its content type. Unsupported formats fall back to JPEG.
    /// </summary>
    public static string FormatToContentType(Format format) => format switch
    {
        Format.Png => PngContentType,
        Format.WebP => WebpContentType,
        Format.Gif => GifContentType,
        _ => JpegContentType,
    };

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

    /// <summary>
    /// Maps a cached resized-image file extension (including the leading dot) to its format.
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    public static string ExtensionToFormat(string ext) => ext?.TrimStart('.').ToLowerInvariant() switch
    {
        "jpeg" => "jpg",
        "png" => "png",
        "gif" => "gif",
        "webp" => "webp",
        _ => "jpg",
    };
}
