#nullable enable

using NetVips;
using OrchardCore.Media.Core.Processing;

namespace OrchardCore.Media.Processing;

/// <summary>
/// The default <see cref="IImageProcessingEngine"/>, backed by libvips through NetVips.
/// </summary>
internal sealed class VipsImageProcessingEngine : IImageProcessingEngine
{
    // Used as an "unbounded" axis when only one dimension is constrained. Media dimensions
    // are bounded well below this by MediaOptions, so it never clamps a real request.
    private const int UnboundedDimension = 1_000_000;

    public Task<ImageProcessingResult> ProcessAsync(
        Stream input,
        ImageProcessingCommands commands,
        CancellationToken cancellationToken = default)
    {
        var buffer = ReadAllBytes(input);

        // NetVips operations are synchronous native calls — offload to the thread pool.
        return Task.Run(() => Process(buffer, commands), cancellationToken);
    }

    private static ImageProcessingResult Process(byte[] buffer, ImageProcessingCommands commands)
    {
        var width = commands.Width ?? 0;
        var height = commands.Height ?? 0;
        var autoOrient = commands.AutoOrient;

        var (formatString, contentType) = ResolveFormat(commands);

        // Dispose the resized image even if WriteToBuffer throws, to release native memory.
        using var resized = Resize(buffer, commands, width, height, autoOrient);
        var outputBytes = resized.WriteToBuffer(formatString);

        return new ImageProcessingResult
        {
            Output = new MemoryStream(outputBytes),
            ContentType = contentType,
        };
    }

    private static Image Resize(byte[] buffer, ImageProcessingCommands commands, int width, int height, bool autoOrient)
    {
        if (width == 0 && height == 0)
        {
            // No resize requested — just decode (honoring orientation) and re-encode.
            using var source = Image.NewFromBuffer(buffer);
            return autoOrient ? source.Autorot() : source.Copy();
        }

        return commands.ResizeMode switch
        {
            ResizeMode.Stretch => ApplyStretch(buffer, width, height, autoOrient),
            ResizeMode.Crop    => ApplyCrop(buffer, width, height, commands.FocalPointX, commands.FocalPointY, autoOrient),
            ResizeMode.Pad     => ApplyPad(buffer, width, height, commands.BackgroundColor, autoOrient, boxPad: false),
            ResizeMode.BoxPad  => ApplyPad(buffer, width, height, commands.BackgroundColor, autoOrient, boxPad: true),
            ResizeMode.Min     => ApplyMax(buffer, width, height, autoOrient),
            _                  => ApplyMax(buffer, width, height, autoOrient),
        };
    }

    // Fits the image within the bounding box, preserving aspect ratio and never upscaling.
    // Uses shrink-on-load from the encoded buffer, which is the fast path for downscaling.
    private static Image ApplyMax(byte[] buffer, int width, int height, bool autoOrient)
    {
        return Image.ThumbnailBuffer(
            buffer,
            width > 0 ? width : UnboundedDimension,
            height: height > 0 ? height : UnboundedDimension,
            size: Enums.Size.Down,
            noRotate: !autoOrient,
            crop: Enums.Interesting.None);
    }

    private static Image ApplyStretch(byte[] buffer, int width, int height, bool autoOrient)
    {
        if (width > 0 && height > 0)
        {
            // Force resampling to the exact dimensions, ignoring aspect ratio.
            return Image.ThumbnailBuffer(
                buffer,
                width,
                height: height,
                size: Enums.Size.Force,
                noRotate: !autoOrient,
                crop: Enums.Interesting.None);
        }

        // A single missing axis means "leave that axis unchanged", which requires the
        // source dimensions — decode and resample per axis.
        using var source = Image.NewFromBuffer(buffer);
        using var oriented = autoOrient ? source.Autorot() : source.Copy();

        var targetWidth = width > 0 ? width : oriented.Width;
        var targetHeight = height > 0 ? height : oriented.Height;

        return oriented.Resize(
            (double)targetWidth / oriented.Width,
            vscale: (double)targetHeight / oriented.Height,
            kernel: Enums.Kernel.Lanczos3);
    }

    private static Image ApplyCrop(byte[] buffer, int width, int height, float? focalX, float? focalY, bool autoOrient)
    {
        // Fast path: both dimensions known and a centered crop — let libvips cover-and-crop
        // during shrink-on-load.
        if (width > 0 && height > 0 && focalX is null && focalY is null)
        {
            return Image.ThumbnailBuffer(
                buffer,
                width,
                height: height,
                size: Enums.Size.Down,
                noRotate: !autoOrient,
                crop: Enums.Interesting.Centre);
        }

        // Focal-point (or single-axis) crop: decode, scale to cover, then extract.
        using var source = Image.NewFromBuffer(buffer);
        using var oriented = autoOrient ? source.Autorot() : source.Copy();

        var targetWidth = width > 0 ? width : oriented.Width;
        var targetHeight = height > 0 ? height : oriented.Height;

        // Scale to cover: the larger axis ratio determines the scale.
        var scale = Math.Max((double)targetWidth / oriented.Width, (double)targetHeight / oriented.Height);
        using var scaled = scale == 1.0 ? oriented.Copy() : oriented.Resize(scale, kernel: Enums.Kernel.Lanczos3);

        var cropW = Math.Min(targetWidth, scaled.Width);
        var cropH = Math.Min(targetHeight, scaled.Height);

        var fx = Math.Clamp(focalX ?? 0.5f, 0f, 1f);
        var fy = Math.Clamp(focalY ?? 0.5f, 0f, 1f);
        var left = (int)(fx * scaled.Width - cropW / 2.0);
        var top = (int)(fy * scaled.Height - cropH / 2.0);

        left = Math.Clamp(left, 0, scaled.Width - cropW);
        top = Math.Clamp(top, 0, scaled.Height - cropH);

        return scaled.ExtractArea(left, top, cropW, cropH);
    }

    private static Image ApplyPad(byte[] buffer, int width, int height, string? bgColor, bool autoOrient, bool boxPad)
    {
        // BoxPad keeps the image at its original size when it already fits the box (no upscale
        // beyond fitting); both modes never upscale because shrink-on-load uses Size.Down.
        using var thumb = Image.ThumbnailBuffer(
            buffer,
            width > 0 ? width : UnboundedDimension,
            height: height > 0 ? height : UnboundedDimension,
            size: Enums.Size.Down,
            noRotate: !autoOrient,
            crop: Enums.Interesting.None);

        // Pad expands the canvas to the requested box. BoxPad only expands to fit the (possibly
        // smaller) thumbnail, so the canvas is the larger of the requested box and the thumbnail.
        var canvasWidth = width > 0 ? width : thumb.Width;
        var canvasHeight = height > 0 ? height : thumb.Height;

        if (boxPad)
        {
            canvasWidth = Math.Max(thumb.Width, canvasWidth);
            canvasHeight = Math.Max(thumb.Height, canvasHeight);
        }

        if (canvasWidth == thumb.Width && canvasHeight == thumb.Height)
        {
            return thumb.Copy();
        }

        var background = BuildBackground(bgColor, thumb.Bands);
        var x = (canvasWidth - thumb.Width) / 2;
        var y = (canvasHeight - thumb.Height) / 2;

        return thumb.Embed(x, y, canvasWidth, canvasHeight, extend: Enums.Extend.Background, background: background);
    }

    // Builds a background pixel matching the image's band layout so Embed never fails on
    // grayscale (1), grayscale+alpha (2), RGB (3) or RGBA (4) images.
    private static double[] BuildBackground(string? hex, int bands)
    {
        var (r, g, b) = ParseHexColor(hex);

        return bands switch
        {
            1 => [(r + g + b) / 3.0],
            2 => [(r + g + b) / 3.0, 255],
            4 => [r, g, b, 255],
            _ => [r, g, b],
        };
    }

    private static (int R, int G, int B) ParseHexColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return (255, 255, 255);
        }

        hex = hex.TrimStart('#');
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }
        if (hex.Length != 6)
        {
            return (255, 255, 255);
        }

        return (
            Convert.ToInt32(hex[0..2], 16),
            Convert.ToInt32(hex[2..4], 16),
            Convert.ToInt32(hex[4..6], 16));
    }

    private static (string FormatString, string ContentType) ResolveFormat(ImageProcessingCommands commands)
    {
        var quality = commands.Quality ?? 0;
        if (quality is <= 0 or > 100)
        {
            quality = 85;
        }

        return commands.Format switch
        {
            Format.Png  => (".png", MediaResizingConstants.PngContentType),
            Format.Gif  => (".gif", MediaResizingConstants.GifContentType),
            Format.WebP => ($".webp[Q={quality}]", MediaResizingConstants.WebpContentType),
            _           => ($".jpg[Q={quality}]", MediaResizingConstants.JpegContentType),
        };
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        if (stream is MemoryStream ms)
        {
            return ms.ToArray();
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
