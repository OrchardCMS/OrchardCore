namespace OrchardCore.FileStorage;

/// <summary>
/// Coordinates <see cref="IFileEventHandler"/> instances for user-uploaded files.
/// </summary>
public sealed class FileCreationService
{
    private readonly IEnumerable<IFileEventHandler> _handlers;

    public FileCreationService(IEnumerable<IFileEventHandler> handlers)
    {
        _handlers = handlers;
    }

    /// <summary>
    /// Runs the pre-create pipeline and returns the stream that should be stored.
    /// The caller owns the original <paramref name="stream"/> and should dispose the returned
    /// <see cref="FileCreatingResult"/> to clean up any replacement stream created by handlers.
    /// </summary>
    public async Task<FileCreatingResult> CreateAsync(
        FileCreatingContext context,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(stream);

        var currentStream = stream;

        try
        {
            foreach (var handler in _handlers)
            {
                var creatingStream = currentStream;
                var result = await handler.CreatingAsync(context, creatingStream, cancellationToken);

                if (result is null)
                {
                    throw new InvalidOperationException($"{handler.GetType().Name} returned a null {nameof(FileCreatingResult)}.");
                }

                currentStream = result.Stream ?? creatingStream;

                if (creatingStream != currentStream && creatingStream != stream)
                {
                    await creatingStream.DisposeAsync();
                }

                if (!result.Succeeded)
                {
                    return FileCreatingResult.Create(currentStream, currentStream != stream, result.Errors.ToList());
                }
            }

            return FileCreatingResult.Create(currentStream, currentStream != stream);
        }
        catch
        {
            if (currentStream != stream)
            {
                await currentStream.DisposeAsync();
            }

            throw;
        }
    }

    /// <summary>
    /// Runs the post-create pipeline after the file was stored successfully.
    /// </summary>
    public async Task CreatedAsync(IFileStoreEntry fileInfo, CancellationToken cancellationToken = default)
    {
        if (!_handlers.Any())
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(fileInfo);

        foreach (var handler in _handlers)
        {
            await handler.CreatedAsync(fileInfo, cancellationToken);
        }
    }
}
