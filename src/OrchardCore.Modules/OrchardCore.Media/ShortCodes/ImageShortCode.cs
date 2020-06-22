using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ShortCodes;

namespace OrchardCore.Media.ShortCodes
{
    public class ImageShortCode : IShortCode
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public ImageShortCode(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
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
            // if it doesn't have [media] and [/media]
            // or doesn't have [image] and [/image]

            var hasMediaCode = text.Contains("[media]", StringComparison.OrdinalIgnoreCase) &&
                text.Contains("[/media]", StringComparison.OrdinalIgnoreCase);

            var hasImageCode = text.Contains("[image]", StringComparison.OrdinalIgnoreCase) &&
                text.Contains("[/image]", StringComparison.OrdinalIgnoreCase);


            if (!hasMediaCode && !hasImageCode)
            {
                return new ValueTask<string>(text);
            }

            using (var sb = StringBuilderPool.GetInstance())
            {
                sb.Builder.Append(text);

                var index = -1;
                // When acting recursively track the modifications to the string builder start index.
                var modifierIndex = 0;
                var allIndices = new List<int>();

                // For backwards compatability check for [media]
                // This code can be removed in a future version.
                // [media]
                if (hasMediaCode)
                {
                    while (-1 != (index = text.IndexOf("[media]", index + 1, StringComparison.Ordinal)))
                    {
                        allIndices.Add(index);
                    }

                    foreach (var start in allIndices)
                    {
                        var end = text.IndexOf("[/media]", start, StringComparison.Ordinal);

                        if (end == -1)
                        {
                            continue;
                        }

                        modifierIndex = SubstituteTag(text, sb, start, modifierIndex, end);
                    }
                }

                // [image]
                if (hasImageCode)
                {
                    index = -1;
                    allIndices.Clear();
                    while (-1 != (index = text.IndexOf("[image]", index + 1, StringComparison.Ordinal)))
                    {
                        allIndices.Add(index);
                    }

                    foreach (var start in allIndices)
                    {
                        var end = text.IndexOf("[/image]", start + modifierIndex, StringComparison.Ordinal);

                        if (end == -1)
                        {
                            continue;
                        }

                        modifierIndex += SubstituteTag(text, sb, start, modifierIndex, end);
                    }
                }

                return new ValueTask<string>(sb.ToString());
            }
        }

        private int SubstituteTag(string text, StringBuilderPool sb, int start, int modifierIndex, int end)
        {
            var url = text.Substring(start + 7, end - start - 7);

            // substitute [thetag] with <img>
            sb.Builder.Remove(start + modifierIndex, end - start + 8);

            var publicUrl = _mediaFileStore.MapPathToPublicUrl(url);

            var tag = "<img src=\"" + publicUrl + "\">";
            tag = _htmlSanitizerService.Sanitize(tag);

            sb.Builder.Insert(start + modifierIndex, tag);

            // Return the value the stringbuilder start index has been modified for recursion.
            return publicUrl.Length - url.Length - 3;
        }
    }
}
