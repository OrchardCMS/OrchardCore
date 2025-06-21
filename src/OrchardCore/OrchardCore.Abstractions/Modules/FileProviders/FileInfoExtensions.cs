using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders;

public static class FileInfoExtensions
{
    public static IEnumerable<string> ReadAllLines(this IFileInfo fileInfo)
        => ReadAllLinesAsync(fileInfo).GetAwaiter().GetResult();

    public static async Task<IEnumerable<string>> ReadAllLinesAsync(this IFileInfo fileInfo)
    {
        var lines = new List<string>();

        if (fileInfo?.Exists ?? false)
        {
            var reader = fileInfo.CreateReadStream();
            await using (reader.ConfigureAwait(false))
            {
                using var sr = new StreamReader(reader);

            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
            }
        }

        return lines;
    }
}
