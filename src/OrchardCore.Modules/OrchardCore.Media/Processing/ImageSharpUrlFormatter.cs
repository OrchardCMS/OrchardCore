using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Processing;

public enum ResizeMode
{
    Undefined,
    Max,
    Crop,
    Pad,
    BoxPad,
    Min,
    Stretch,
}

public enum Format
{
    Undefined,
    Bmp,
    Gif,
    Jpg,
    Png,
    Tga,
    WebP,
}

internal sealed class ImageSharpUrlFormatter
{
    public static string GetImageResizeUrl(string path, IDictionary<string, string> queryStringParams = null, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
    {
        if (string.IsNullOrEmpty(path) || (!width.HasValue && !height.HasValue && queryStringParams == null))
        {
            return path;
        }

        var mediaCommands = new MediaCommands();

        if (queryStringParams != null)
        {
            mediaCommands.SetCommands(queryStringParams);
        }

        if (width.HasValue)
        {
            mediaCommands.SetWidth(width.ToString());
        }

        if (height.HasValue)
        {
            mediaCommands.SetHeight(height.ToString());
        }

        if (resizeMode != ResizeMode.Undefined)
        {
            mediaCommands.SetResizeMode(resizeMode.ToString().ToLower());
        }

        // The format is set before quality such that the quality is not
        // invalidated when the url is generated.
        if (format != Format.Undefined)
        {
            mediaCommands.SetFormat(format.ToString().ToLower());
        }

        if (quality.HasValue)
        {
            mediaCommands.SetQuality(quality.ToString());
        }

        if (anchor != null)
        {
            mediaCommands.SetResizeFocalPoint(anchor.X.ToString(CultureInfo.InvariantCulture) + ',' + anchor.Y.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(bgcolor))
        {
            mediaCommands.SetBackgroundColor(bgcolor);
        }

        return QueryHelpers.AddQueryString(path, mediaCommands.GetValues());
    }
}
