namespace OrchardCore.FileStorage;

public sealed class FileCreatingContext
{
    public FileCreatingContext(string path, long? length = null, string contentType = null)
    {
        Path = path;
        Length = length;
        ContentType = contentType;
    }

    public string Path { get; set; }

    public string FileName => System.IO.Path.GetFileName(Path);

    public long? Length { get; set; }

    public string ContentType { get; set; }
}
