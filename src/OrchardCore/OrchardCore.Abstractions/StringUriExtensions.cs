using System.Linq;

namespace System
{
    public static class StringUriExtensions
    {
        public static string ToUriComponents(this string url, UriFormat uriFormat = UriFormat.UriEscaped)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            return uri.GetComponents(UriComponents.SerializationInfoString, uriFormat);
        }

        public static string ToSnakeCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
