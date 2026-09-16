using System.IO.Pipelines;

namespace OrchardCore.Media.Services;

/// <summary>
/// Pluggable abstraction for storing partial TUS upload data.
/// The default implementation stores files on local disk; cloud providers
/// (Azure Blob, Amazon S3) can supply their own implementations.
/// </summary>
public interface ITusTempStore
{
    /// <summary>
    /// Creates a new empty file for the given upload.
    /// </summary>
    Task CreateFileAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Appends data from <paramref name="stream"/> starting at <paramref name="offset"/>.
    /// Returns the number of bytes actually written.
    /// </summary>
    Task<long> AppendDataAsync(string fileId, Stream stream, long offset, CancellationToken cancellationToken);

    /// <summary>
    /// Appends data from <paramref name="pipeReader"/> starting at <paramref name="offset"/>.
    /// Returns the number of bytes actually written.
    /// </summary>
    Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, long offset, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the file and any associated temporary data.
    /// </summary>
    Task DeleteFileAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns whether a file with the given ID exists.
    /// </summary>
    Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Opens the completed upload's data for reading.
    /// Caller is responsible for disposing the stream.
    /// </summary>
    Stream OpenReadStream(string fileId);

    /// <summary>
    /// Gets the length (in bytes) of the stored data.
    /// </summary>
    long GetFileLength(string fileId);
}
