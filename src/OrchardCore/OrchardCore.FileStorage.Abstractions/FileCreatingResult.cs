using OrchardCore.Infrastructure;

namespace OrchardCore.FileStorage;

/// <summary>
/// Represents the outcome of processing a file before it is stored.
/// </summary>
public sealed class FileCreatingResult : Result, IAsyncDisposable
{
    private readonly bool _ownsStream;

    private FileCreatingResult(Stream stream, bool ownsStream)
        : base(true)
    {
        Stream = stream;
        _ownsStream = ownsStream;
    }

    private FileCreatingResult(Stream stream, bool ownsStream, IEnumerable<ResultError> errors)
        : base(errors)
    {
        Stream = stream;
        _ownsStream = ownsStream;
    }

    /// <summary>
    /// Gets the stream that should continue through the upload pipeline.
    /// </summary>
    public Stream Stream { get; }

    public string ErrorMessage
        => Errors
            .Select(error => error.Message?.Value)
            .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));

    public static FileCreatingResult Success(Stream stream)
        => new(stream, false);

    public static FileCreatingResult Failed(Stream stream = null, params ResultError[] errors)
        => new(stream, false, errors);

    internal static FileCreatingResult Create(Stream stream, bool ownsStream, IEnumerable<ResultError> errors)
        => new(stream, ownsStream, errors);

    internal static FileCreatingResult Create(Stream stream, bool ownsStream)
        => new(stream, ownsStream);

    public async ValueTask DisposeAsync()
    {
        if (_ownsStream && Stream is not null)
        {
            await Stream.DisposeAsync();
        }
    }
}
