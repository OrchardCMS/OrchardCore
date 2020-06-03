using System.Collections.Generic;

namespace OrchardCore.ShortCodes
{
    // TODO: This should be replaced by more efficient shortcode parser
    public class ShortCodeParser
    {
        public IEnumerable<ShortCodeOccurence> Parse(string markup)
        {
            var shortCodeStartIndex = -1;
            var shortCodeEndIndex = -1;
            var startIndex = 0;
            var occurences = new List<ShortCodeOccurence>();
            do
            {
                startIndex = markup.IndexOf("[", startIndex);
                if (startIndex == -1)
                {
                    break;
                }

                shortCodeStartIndex = startIndex;

                var endIndex = markup.IndexOf("]", startIndex);
                if (endIndex == -1)
                {
                    break;
                }

                startIndex = markup.IndexOf("[/", startIndex);
                if (startIndex == -1)
                {
                    break;
                }

                endIndex = markup.IndexOf("]", startIndex);
                if (endIndex == -1)
                {
                    break;
                }

                shortCodeEndIndex = endIndex + 1;

                occurences.Add(new ShortCodeOccurence {
                    Name = markup[(shortCodeStartIndex + 1)..markup.IndexOf("]", shortCodeStartIndex)],
                    Text = markup[shortCodeStartIndex..shortCodeEndIndex],
                    Location = new ShortCodeSpan(shortCodeStartIndex, shortCodeEndIndex - shortCodeStartIndex)
                });
                startIndex = endIndex;
            } while (startIndex <= markup.Length);

            return occurences;
        }
    }
}
