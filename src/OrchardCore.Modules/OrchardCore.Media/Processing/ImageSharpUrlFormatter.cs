using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.Processing
{
    public enum ResizeMode
    {
        Undefined,
        Max,
        Crop,
        Pad,
        BoxPad,
        Min,
        Stretch
    }

    public enum Format
    {
        Undefined,
        Bmp,
        Gif,
        Jpg,
        Png,
        Tga,
        WebP
    }

    internal class ImageSharpUrlFormatter
    {
        public static string GetImageResizeUrl(string path, IDictionary<string, string> queryStringParams = null, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, int? quality = null, Format format = Format.Undefined, Anchor anchor = null, string bgcolor = null)
        {
            if (string.IsNullOrEmpty(path) || (!width.HasValue && !height.HasValue && queryStringParams == null))
            {
                return path;
            }

            if (queryStringParams == null)
            {
                queryStringParams = new Dictionary<string, string>();
            }

            if (width.HasValue)
            {
                queryStringParams["width"] = width.ToString();
            }

            if (height.HasValue)
            {
                queryStringParams["height"] = height.ToString();
            }

            if (resizeMode != ResizeMode.Undefined)
            {
                queryStringParams["rmode"] = resizeMode.ToString().ToLower();
            }

            if (quality.HasValue)
            {
                queryStringParams["quality"] = quality.ToString();
            }

            if (format != Format.Undefined)
            {
                queryStringParams["format"] = format.ToString().ToLower();
            }

            if (anchor != null)
            {
                queryStringParams["rxy"] = anchor.X.ToString(CultureInfo.InvariantCulture) + ',' + anchor.Y.ToString(CultureInfo.InvariantCulture);
            }

            if (!String.IsNullOrEmpty(bgcolor))
            {
                queryStringParams["bgcolor"] = bgcolor;
            }

            return QueryHelpers.AddQueryString(path, queryStringParams);
        }
    }
}
