using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Text;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace OrchardCore.Media.Indexing;

public class PresentationDocumentMediaFileTextProvider : IMediaFileTextProvider
{
    public Task<string> GetTextAsync(string path, Stream fileStream)
    {
        try
        {
            using var document = PresentationDocument.Open(fileStream, false);

            var slideIds = document.PresentationPart?.Presentation?.SlideIdList?.ChildElements.Cast<SlideId>();
            if (slideIds is null || !slideIds.Any())
            {
                return Task.FromResult(String.Empty);
            }

            using var stringBuilder = ZString.CreateStringBuilder();

            foreach (var slideId in slideIds)
            {
                var slidePart = document.PresentationPart.GetPartById(slideId.RelationshipId) as SlidePart;

                if (slidePart == null)
                {
                    continue;
                }

                var slideText = GetText(slidePart);

                stringBuilder.AppendLine(slideText);
            }

            return Task.FromResult(stringBuilder.ToString());
        }
        catch
        {
            return Task.FromResult(String.Empty);
        }
    }

    private static string GetText(SlidePart slidePart)
    {
        using var stringBuilder = ZString.CreateStringBuilder();

        foreach (var paragraph in slidePart.Slide.Descendants<Paragraph>())
        {
            foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
            {
                stringBuilder.Append(text.Text);
            }
        }

        return stringBuilder.ToString();
    }
}
