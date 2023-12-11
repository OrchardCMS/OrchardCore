namespace OrchardCore.Tests.Utilities
{
    public static class FileInfoExtensions
    {
        public static string ReadToEnd(this IFileInfo fileInfo)
            => ReadToEndAsync(fileInfo).GetAwaiter().GetResult();

        public static async Task<string> ReadToEndAsync(this IFileInfo fileInfo)
        {
            await using var stream = fileInfo.CreateReadStream();

            using var streamReader = new StreamReader(stream);

            return await streamReader.ReadToEndAsync();
        }
    }
}
