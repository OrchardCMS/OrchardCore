namespace OrchardCore.Tests.Utilities;

public static class FileInfoExtensions
{
    public static string ReadToEnd(this IFileInfo fileInfo)
    {
        using var stream = fileInfo.CreateReadStream();

        using var streamReader = new StreamReader(stream);

        return streamReader.ReadToEnd();
    }

    public static async Task<string> ReadToEndAsync(this IFileInfo fileInfo)
    {
        var stream = fileInfo.CreateReadStream();
        await using (stream.ConfigureAwait(false))
        {
            using var streamReader = new StreamReader(stream);

        return await streamReader.ReadToEndAsync();
        }
    }
}
