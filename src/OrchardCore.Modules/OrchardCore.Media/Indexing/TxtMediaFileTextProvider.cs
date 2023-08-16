using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Media.Indexing;

public class TxtMediaFileTextProvider : IMediaFileTextProvider
{
    public async Task<string> GetTextAsync(string path, Stream fileStream)
    {
        using (StreamReader reader = new StreamReader(fileStream))
        {
            return await reader.ReadToEndAsync();
        }
    }
}
