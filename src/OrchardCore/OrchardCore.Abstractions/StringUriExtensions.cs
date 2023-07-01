namespace System
{
    public static class StringUriExtensions
    {
        public static string ToUriComponents(this string url, UriFormat uriFormat = UriFormat.UriEscaped)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            return uri.GetComponents(UriComponents.SerializationInfoString, uriFormat);
        }
    }
}
