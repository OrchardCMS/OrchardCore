namespace OrchardCore.Tests.Utilities
{
    public static class FileInfoExtensions
    {
        public static string ReadToEnd(this IFileInfo fileInfo)
        {
            using var stream = fileInfo.CreateReadStream();
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
