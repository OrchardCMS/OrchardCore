#nullable enable

using System.Globalization;
using NetVips;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Processing;

internal sealed class VipsImageProcessingEngine : IImageProcessingEngine
{
    public Task<ImageProcessingResult> ProcessAsync(
        Stream input,
        MediaCommands commands,
        CancellationToken cancellationToken = default)
    {
        // NetVips operations are synchronous native calls — offload to thread pool.
        return Task.Run(() =>
        {
            var buffer = ReadAllBytes(input);
            using var source = Image.NewFromBuffer(buffer);
            using var oriented = source.Autorot();

            int.TryParse(commands.Width, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width);
            int.TryParse(commands.Height, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height);

            Image resized;
            if (width == 0 && height == 0)
            {
                resized = oriented.Copy();
            }
            else
            {
                var mode = Enum.TryParse<ResizeMode>(commands.ResizeMode, ignoreCase: true, out var m)
                    ? m
                    : ResizeMode.Max;

                resized = mode switch
                {
                    ResizeMode.Stretch => ApplyStretch(oriented, width, height),
                    ResizeMode.Crop    => ApplyCrop(oriented, width, height, commands.ResizeFocalPoint),
                    ResizeMode.Pad     => ApplyPad(oriented, width, height, commands.BackgroundColor),
                    ResizeMode.BoxPad  => ApplyPad(oriented, width, height, commands.BackgroundColor),
                    ResizeMode.Min     => ApplyMin(oriented, width, height),
                    _                  => ApplyMax(oriented, width, height),
                };
            }

            var (formatString, contentType) = ResolveFormat(commands);
            var outputBytes = resized.WriteToBuffer(formatString);
            resized.Dispose();

            return new ImageProcessingResult
            {
                Output = new MemoryStream(outputBytes),
                ContentType = contentType,
            };
        }, cancellationToken);
    }

    private static Image ApplyMax(Image image, int width, int height)
    {
        // thumbnail_image requires a positive width. When only height is constrained,
        // derive a width from the aspect ratio so libvips scales by height instead.
        if (width == 0)
        {
            width = height > 0
                ? (int)Math.Ceiling((double)image.Width * height / image.Height)
                : image.Width;
        }

        return image.ThumbnailImage(
            width,
            height: height > 0 ? height : null,
            size: Enums.Size.Down,
            crop: Enums.Interesting.None);
    }

    private static Image ApplyMin(Image image, int width, int height)
    {
        // Scale so the SMALLER axis matches the target — opposite of Max.
        if (width == 0)
        {
            return image.ThumbnailImage(image.Width, height: height, size: Enums.Size.Down, crop: Enums.Interesting.None);
        }
        if (height == 0)
        {
            return image.ThumbnailImage(width, size: Enums.Size.Down, crop: Enums.Interesting.None);
        }

        var scale = Math.Min((double)width / image.Width, (double)height / image.Height);
        if (scale >= 1.0)
        {
            return image.Copy();
        }

        return image.Resize(scale, kernel: Enums.Kernel.Lanczos3);
    }

    private static Image ApplyStretch(Image image, int width, int height)
    {
        if (width == 0)
        {
            width = image.Width;
        }
        if (height == 0)
        {
            height = image.Height;
        }
        return image.Resize(
            (double)width / image.Width,
            vscale: (double)height / image.Height,
            kernel: Enums.Kernel.Lanczos3);
    }

    private static Image ApplyCrop(Image image, int width, int height, string? focalPoint)
    {
        if (width == 0)
        {
            width = image.Width;
        }
        if (height == 0)
        {
            height = image.Height;
        }

        // Scale to cover: the larger axis ratio determines the scale.
        var scale = Math.Max((double)width / image.Width, (double)height / image.Height);
        var scaled = scale == 1.0 ? image.Copy() : image.Resize(scale, kernel: Enums.Kernel.Lanczos3);

        var cropW = Math.Min(width, scaled.Width);
        var cropH = Math.Min(height, scaled.Height);

        var (fx, fy) = ParseFocalPoint(focalPoint);
        var left = (int)(fx * scaled.Width  - cropW / 2.0);
        var top  = (int)(fy * scaled.Height - cropH / 2.0);

        left = Math.Clamp(left, 0, scaled.Width  - cropW);
        top  = Math.Clamp(top,  0, scaled.Height - cropH);

        var result = scaled.ExtractArea(left, top, cropW, cropH);
        scaled.Dispose();
        return result;
    }

    private static Image ApplyPad(Image image, int width, int height, string? bgColor)
    {
        if (width == 0)
        {
            width = image.Width;
        }
        if (height == 0)
        {
            height = image.Height;
        }

        using var thumb = image.ThumbnailImage(width, height: height, size: Enums.Size.Down, crop: Enums.Interesting.None);

        var bg = ParseHexColor(bgColor);
        var x = (width  - thumb.Width)  / 2;
        var y = (height - thumb.Height) / 2;

        return thumb.Embed(x, y, width, height, extend: Enums.Extend.Background, background: bg);
    }

    private static (float x, float y) ParseFocalPoint(string? fp)
    {
        if (string.IsNullOrEmpty(fp))
        {
            return (0.5f, 0.5f);
        }

        var comma = fp.IndexOf(',');
        if (comma < 1)
        {
            return (0.5f, 0.5f);
        }

        if (float.TryParse(fp.AsSpan(0, comma), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(fp.AsSpan(comma + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return (Math.Clamp(x, 0f, 1f), Math.Clamp(y, 0f, 1f));
        }

        return (0.5f, 0.5f);
    }

    private static double[] ParseHexColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return [255, 255, 255];
        }

        hex = hex.TrimStart('#');
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }
        if (hex.Length != 6)
        {
            return [255, 255, 255];
        }

        return
        [
            Convert.ToInt32(hex[0..2], 16),
            Convert.ToInt32(hex[2..4], 16),
            Convert.ToInt32(hex[4..6], 16),
        ];
    }

    private static (string FormatString, string ContentType) ResolveFormat(MediaCommands commands)
    {
        int.TryParse(commands.Quality, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quality);
        if (quality is <= 0 or > 100)
        {
            quality = 85;
        }

        return commands.Format?.ToLowerInvariant() switch
        {
            "png"  => (".png",                $"image/png"),
            "gif"  => (".gif",                $"image/gif"),
            "bmp"  => (".bmp",                $"image/bmp"),
            "webp" => ($".webp[Q={quality}]", $"image/webp"),
            _      => ($".jpg[Q={quality}]",  $"image/jpeg"),
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
