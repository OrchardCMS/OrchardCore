using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Processing;

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
            mediaCommands.Width = width.ToString();
        }

        if (height.HasValue)
        {
            mediaCommands.Height = height.ToString();
        }

        if (resizeMode != ResizeMode.Undefined)
        {
            mediaCommands.ResizeMode = resizeMode.ToString().ToLower();
        }

        // The format is set before quality such that the quality is not
        // invalidated when the url is generated.
        if (format != Format.Undefined)
        {
            mediaCommands.Format = format.ToString().ToLower();
        }

        if (quality.HasValue)
        {
            mediaCommands.Quality = quality.ToString();
        }

        if (anchor != null)
        {
            mediaCommands.ResizeFocalPoint = anchor.X.ToString(CultureInfo.InvariantCulture) + ',' + anchor.Y.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrEmpty(bgcolor))
        {
            mediaCommands.BackgroundColor = bgcolor;
        }

        return QueryHelpers.AddQueryString(path, mediaCommands.GetValues());
    }
}
