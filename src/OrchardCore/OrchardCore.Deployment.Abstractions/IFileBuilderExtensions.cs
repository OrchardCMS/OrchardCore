namespace OrchardCore.Deployment;

public static class IFileBuilderExtensions
{
    public static Task SetFileAsync(this IFileBuilder fileBuilder, string subpath, byte[] content)
    {
        using var stream = new MemoryStream(content);

        return fileBuilder.SetFileAsync(subpath, stream);
    }
}
