using System;

namespace OrchardCore.ShortCodes.Extensions
{
    public static class ShortCodeContentExtensions
    {
        public static ShortCodeContent AppendFormat(this ShortCodeContent shortCodeContent, string format, params object[] args)
            => shortCodeContent.Append(string.Format(format, args));

        public static ShortCodeContent AppendFormat(this ShortCodeContent shortCodeContent, IFormatProvider provider, string format, params object[] args)
            => shortCodeContent.Append(string.Format(provider, format, args));
    }
}
