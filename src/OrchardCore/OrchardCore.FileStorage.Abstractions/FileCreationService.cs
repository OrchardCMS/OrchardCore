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
    /// By default, the returned <see cref="FileCreatingResult"/> owns the final stream and disposes it
    /// when the result is disposed.
    /// </summary>
    public Task<FileCreatingResult> CreateAsync(
        FileCreatingContext context,
        Stream stream,
        CancellationToken cancellationToken = default)
        => CreateAsync(context, stream, leaveOpen: false, cancellationToken);

    /// <summary>
    /// Runs the pre-create pipeline and returns the stream that should be stored.
    /// By default, the returned <see cref="FileCreatingResult"/> owns the final stream and disposes it
    /// when the result is disposed. Set <paramref name="leaveOpen"/> to <see langword="true"/> to keep
    /// the original <paramref name="stream"/> owned by the caller when handlers do not replace it.
    /// </summary>
    public async Task<FileCreatingResult> CreateAsync(
        FileCreatingContext context,
        Stream stream,
        bool leaveOpen = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(stream);

        var currentStream = stream;
        var ownsOriginalStream = !leaveOpen;

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

                if (creatingStream != currentStream)
                {
                    if (creatingStream == stream)
                    {
                        if (ownsOriginalStream)
                        {
                            await creatingStream.DisposeAsync();
                            ownsOriginalStream = false;
                        }
                    }
                    else
                    {
                        await creatingStream.DisposeAsync();
                    }
                }

                if (!result.Succeeded)
                {
                    return FileCreatingResult.Create(currentStream, OwnsResultStream(stream, currentStream, ownsOriginalStream), result.Errors.ToList());
                }
            }

            return FileCreatingResult.Create(currentStream, OwnsResultStream(stream, currentStream, ownsOriginalStream));
        }
        catch
        {
            if (OwnsResultStream(stream, currentStream, ownsOriginalStream))
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

    private static bool OwnsResultStream(Stream originalStream, Stream currentStream, bool ownsOriginalStream)
        => currentStream is not null && (currentStream != originalStream || ownsOriginalStream);
}
