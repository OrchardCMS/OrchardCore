using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OrchardCore.Media.Indexing;

public class WordDocumentMediaFileTextProvider : IMediaFileTextProvider
{
    public Task<string> GetTextAsync(string path, Stream fileStream)
    {
        try
        {
            using var document = WordprocessingDocument.Open(fileStream, false);

            var paragraphs = document.MainDocumentPart?.Document?.Body?.Descendants<Paragraph>();
            if (paragraphs is null || !paragraphs.Any())
            {
                return Task.FromResult(String.Empty);
            }

            var stringBuilder = new StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                stringBuilder.AppendLine(paragraph.InnerText);
            }

            return Task.FromResult(stringBuilder.ToString());
        }
        catch
        {
            return Task.FromResult(String.Empty);
        }
    }
}
