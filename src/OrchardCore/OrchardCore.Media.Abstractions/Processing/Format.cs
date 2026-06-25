namespace OrchardCore.Media.Core.Processing;

public enum Format
{
    Undefined,

    [Obsolete("BMP output is no longer supported by the image processing engine. Requests for this format fall back to JPEG.")]
    Bmp,
    Gif,
    Jpg,
    Png,

    [Obsolete("TGA output is no longer supported by the image processing engine. Requests for this format fall back to JPEG.")]
    Tga,
    WebP,
}
