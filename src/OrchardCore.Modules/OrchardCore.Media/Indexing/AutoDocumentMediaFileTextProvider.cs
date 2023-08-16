using System;
using System.IO;
using System.Threading.Tasks;
using Spire.Doc;

namespace OrchardCore.Media.Indexing;

public class AutoDocumentMediaFileTextProvider : IMediaFileTextProvider
{
    public async Task<string> GetTextAsync(string path, Stream fileStream)
    {
        try
        {
            using var doc = new Document();

            doc.LoadFromStream(fileStream, FileFormat.Auto);

            using var memoryStream = new MemoryStream();

            doc.SaveToStream(memoryStream, FileFormat.Txt);

            memoryStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(memoryStream);
            var content = await reader.ReadToEndAsync();

            return content;
        }
        catch
        {
            return String.Empty;
        }
    }
}
