namespace OrchardCore.Deployment;

public interface IFileBuilder
{
    Task SetFileAsync(string subpath, Stream stream);
}
