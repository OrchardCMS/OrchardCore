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
            await using var reader = fileInfo.CreateReadStream();
            using var sr = new StreamReader(reader);

            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
        }

        return lines;
    }
}
