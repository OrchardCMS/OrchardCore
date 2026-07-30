namespace OrchardCore.FileStorage;

public interface IFileEventHandler
{
    Task<FileCreatingResult> CreatingAsync(FileCreatingContext context, Stream stream, CancellationToken cancellationToken = default);

    Task CreatedAsync(IFileStoreEntry fileInfo, CancellationToken cancellationToken = default);
}
