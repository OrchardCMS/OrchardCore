using System.Web;

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

    internal class ImageSharpUrlFormatter
    {
        public static string GetImageResizeUrl(string path, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)
        {
            if (string.IsNullOrEmpty(path) || (!width.HasValue && !height.HasValue))
            {
                return path;
            }

            var pathParts = path.Split('?');
            var query = HttpUtility.ParseQueryString(pathParts.Length > 1 ? pathParts[1] : string.Empty);

            if (width.HasValue)
            {
                query["width"] = width.ToString();
            }

            if (height.HasValue)
            {
                query["height"] = height.ToString();
            }

            if (resizeMode != ResizeMode.Undefined)
            {
                query["rmode"] = resizeMode.ToString().ToLower();
            }

            return $"{pathParts[0]}?{query.ToString()}";
        }
    }
}
