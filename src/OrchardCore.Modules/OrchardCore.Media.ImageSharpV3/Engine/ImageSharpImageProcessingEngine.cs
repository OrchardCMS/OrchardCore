#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Processing;
using ISImage = SixLabors.ImageSharp.Image;
using ISResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using OCResizeMode = OrchardCore.Media.Core.Processing.ResizeMode;

namespace OrchardCore.Media.ImageSharpV3.Engine;

/// <summary>
/// An <see cref="IImageProcessingEngine"/> backed by SixLabors.ImageSharp (v3). Registered in place
/// of the default NetVips engine when the <c>OrchardCore.Media.ImageSharpV3</c> feature is enabled.
/// </summary>
internal sealed class ImageSharpImageProcessingEngine : IImageProcessingEngine
{
    private const int DefaultQuality = 85;

    public async Task<ImageProcessingResult> ProcessAsync(
        Stream input,
        ImageProcessingCommands commands,
        CancellationToken cancellationToken = default)
    {
        using var image = await ISImage.LoadAsync(input, cancellationToken);

        if (commands.AutoOrient)
        {
            image.Mutate(x => x.AutoOrient());
        }

        var width = commands.Width ?? 0;
        var height = commands.Height ?? 0;

        if (width > 0 || height > 0)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = MapResizeMode(commands.ResizeMode),
            };

            if (commands.FocalPointX is not null || commands.FocalPointY is not null)
            {
                resizeOptions.CenterCoordinates = new PointF(
                    commands.FocalPointX ?? 0.5f,
                    commands.FocalPointY ?? 0.5f);
            }

            if (resizeOptions.Mode is ISResizeMode.Pad or ISResizeMode.BoxPad)
            {
                resizeOptions.PadColor = ParseColor(commands.BackgroundColor);
            }

            image.Mutate(x => x.Resize(resizeOptions));
        }

        var (encoder, contentType) = ResolveEncoder(commands);

        var output = new MemoryStream();
        await image.SaveAsync(output, encoder, cancellationToken);
        output.Position = 0;

        return new ImageProcessingResult
        {
            Output = output,
            ContentType = contentType,
        };
    }

    private static ISResizeMode MapResizeMode(OCResizeMode mode) => mode switch
    {
        OCResizeMode.Crop => ISResizeMode.Crop,
        OCResizeMode.Pad => ISResizeMode.Pad,
        OCResizeMode.BoxPad => ISResizeMode.BoxPad,
        OCResizeMode.Min => ISResizeMode.Min,
        OCResizeMode.Stretch => ISResizeMode.Stretch,
        _ => ISResizeMode.Max,
    };

    private static (IImageEncoder Encoder, string ContentType) ResolveEncoder(ImageProcessingCommands commands)
    {
        var quality = commands.Quality is > 0 and <= 100 ? commands.Quality.Value : DefaultQuality;

        return commands.Format switch
        {
            Format.Png => (new PngEncoder(), MediaResizingConstants.PngContentType),
            Format.Gif => (new GifEncoder(), MediaResizingConstants.GifContentType),
            Format.WebP => (new WebpEncoder { Quality = quality }, MediaResizingConstants.WebpContentType),
            _ => (new JpegEncoder { Quality = quality }, MediaResizingConstants.JpegContentType),
        };
    }

    private static Color ParseColor(string? hex)
    {
        if (!string.IsNullOrWhiteSpace(hex) && Color.TryParseHex(hex, out var color))
        {
            return color;
        }

        return Color.White;
    }
}
