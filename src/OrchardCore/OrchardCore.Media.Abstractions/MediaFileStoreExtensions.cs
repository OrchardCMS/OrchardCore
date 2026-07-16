using OrchardCore.FileStorage;

namespace OrchardCore.Media;

/// <summary>
/// Provides Orchard Core's built-in safe upload flow for media files.
/// </summary>
public static class MediaFileStoreExtensions
{
    /// <summary>
    /// Runs the file event pipeline before invoking the media store's creation pipeline.
    /// </summary>
    public static async Task<string> CreateFileFromStreamAsync(
        this IMediaFileStore mediaFileStore,
        FileCreationService fileCreationService,
        string path,
        Stream stream,
        bool overwrite = false,
        long? length = null,
        string contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mediaFileStore);
        ArgumentNullException.ThrowIfNull(fileCreationService);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(stream);

        var fileCreatingContext = new FileCreatingContext(path, length, contentType);

        await using var fileCreatingResult = await fileCreationService.CreateAsync(
            fileCreatingContext,
            stream,
            leaveOpen: true,
            cancellationToken);

        if (!fileCreatingResult.Succeeded)
        {
            throw new FileStoreException(fileCreatingResult.ErrorMessage ?? $"The file '{fileCreatingContext.FileName}' was rejected.");
        }

        var createdPath = await mediaFileStore.CreateFileFromStreamAsync(path, fileCreatingResult.Stream, overwrite);
        var fileInfo = await mediaFileStore.GetFileInfoAsync(createdPath);

        await fileCreationService.CreatedAsync(fileInfo, cancellationToken);

        return createdPath;
    }
}
