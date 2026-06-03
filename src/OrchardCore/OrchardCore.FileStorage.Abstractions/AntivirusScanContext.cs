namespace OrchardCore.FileStorage;

public sealed class AntivirusScanContext
{
    public AntivirusScanContext(string path, long? length = null, string contentType = null)
    {
        Path = path;
        Length = length;
        ContentType = contentType;
    }

    public string Path { get; }

    public string FileName => System.IO.Path.GetFileName(Path);

    public long? Length { get; }

    public string ContentType { get; }
}
