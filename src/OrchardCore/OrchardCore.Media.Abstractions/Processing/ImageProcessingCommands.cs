using OrchardCore.Media.Core.Processing;

namespace OrchardCore.Media.Processing;

/// <summary>
/// The fully parsed, engine-agnostic set of instructions for transforming a single image.
/// The media pipeline produces this from the validated request, so an <see cref="IImageProcessingEngine"/>
/// never deals with query strings, tokens or formatting details.
/// </summary>
public sealed class ImageProcessingCommands
{
    /// <summary>
    /// Gets or sets the requested output width in pixels, or <see langword="null"/> when the width
    /// is unconstrained.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the requested output height in pixels, or <see langword="null"/> when the height
    /// is unconstrained.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the resize mode that controls how the image is fitted to the requested
    /// dimensions. Defaults to <see cref="ResizeMode.Max"/>.
    /// </summary>
    public ResizeMode ResizeMode { get; set; } = ResizeMode.Max;

    /// <summary>
    /// Gets or sets the target output format. <see cref="Format.Undefined"/> lets the engine choose
    /// its default encoder (JPEG).
    /// </summary>
    public Format Format { get; set; } = Format.Undefined;

    /// <summary>
    /// Gets or sets the output quality (1-100) for lossy formats such as JPEG and WebP, or
    /// <see langword="null"/> to let the engine apply its default.
    /// </summary>
    public int? Quality { get; set; }

    /// <summary>
    /// Gets or sets the background color used to fill padding for the <see cref="ResizeMode.Pad"/>
    /// and <see cref="ResizeMode.BoxPad"/> modes, as a hexadecimal RGB string (for example
    /// <c>ffffff</c>). <see langword="null"/> lets the engine use its default (white).
    /// </summary>
    public string BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the horizontal focal point (0.0-1.0) used when cropping, or <see langword="null"/>
    /// to center horizontally.
    /// </summary>
    public float? FocalPointX { get; set; }

    /// <summary>
    /// Gets or sets the vertical focal point (0.0-1.0) used when cropping, or <see langword="null"/>
    /// to center vertically.
    /// </summary>
    public float? FocalPointY { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image should be auto-oriented from its EXIF
    /// orientation metadata before processing. Defaults to <see langword="true"/>.
    /// </summary>
    public bool AutoOrient { get; set; } = true;
}
