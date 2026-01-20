using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Media.Indexing;

public class TextMediaFileTextProvider : IMediaFileTextProvider
{
    public async Task<string> GetTextAsync(string path, Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);

        return await reader.ReadToEndAsync();
    }
}
