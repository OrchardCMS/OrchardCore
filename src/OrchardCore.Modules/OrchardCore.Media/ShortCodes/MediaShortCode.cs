using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ShortCodes;

namespace OrchardCore.Media.ShortCodes
{
    public class MediaShortCode : IShortCode
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public MediaShortCode(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
        }

        public ValueTask<string> ProcessAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ValueTask<string>(string.Empty);
            }

            // optimize code path if nothing to do
            if (!text.Contains("[media]", StringComparison.OrdinalIgnoreCase) || !text.Contains("[/media]", StringComparison.OrdinalIgnoreCase))
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

                    var tag = "<img src=\"" + url + "\">";
                    tag = _htmlSanitizerService.Sanitize(tag);

                    sb.Builder.Insert(start, tag);
                }

                return new ValueTask<string>(sb.ToString());
            }
        }
    }
}
