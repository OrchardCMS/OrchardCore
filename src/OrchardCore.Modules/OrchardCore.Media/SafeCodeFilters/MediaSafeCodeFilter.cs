using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using OrchardCore.DisplayManagement;
using OrchardCore.Infrastructure.SafeCodeFilters;

namespace OrchardCore.Media.SafeCodeFilters
{
    public class MediaSafeCodeFilter : ISafeCodeFilter
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaSafeCodeFilter(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public ValueTask<string> ProcessAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ValueTask<string>(string.Empty);
            }

            // optimize code path if nothing to do
            if (!text.Contains("[media]", StringComparison.OrdinalIgnoreCase) && !text.Contains("[/media]", StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<string>(text);
            }

            using (var sb = StringBuilderPool.GetInstance())
            {
                sb.Builder.Append(text);

                var index = -1;
                var allIndexes = new List<int>();

                // [media]

                while (-1 != (index = text.IndexOf("[media]", index + 1, StringComparison.Ordinal)))
                {
                    allIndexes.Add(index);
                }

                foreach (var start in allIndexes)
                {
                    var end = text.IndexOf("[/media]", start, StringComparison.Ordinal);

                    if (end == -1)
                    {
                        continue;
                    }

                    var url = text.Substring(start + 7, end - start - 7);

                    // substitue [media] with <img>
                    sb.Builder.Remove(start, end - start + 8);

                    url = _mediaFileStore.MapPathToPublicUrl(url);

                    // TODO Is this enough?
                    // The JavascriptEncoder (DI) by default does not handle diacritics
                    // It can be configured to support different code pages.
                    url = HttpUtility.JavaScriptStringEncode(url);

                    // TODO Alternative might be to html sanitize the tag.
                    var tag = String.Format("<img src=\"{0}\">", url);

                    sb.Builder.Insert(start, tag);
                }

                return new ValueTask<string>(sb.ToString());
            }
        }
    }
}
