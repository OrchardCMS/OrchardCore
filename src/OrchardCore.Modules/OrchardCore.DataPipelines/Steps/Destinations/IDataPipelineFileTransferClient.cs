namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A connection to a file server, such as an FTP or SFTP server, that the upload steps send files to with
/// <see cref="DataPipelineRemotePaths.UploadAsync"/>. Connect it, then upload the files. Disposing it closes the
/// connection.
/// </summary>
public interface IDataPipelineFileTransferClient : IAsyncDisposable
{
    /// <summary>
    /// Connects to the server and authenticates.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the client is connected.</returns>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a folder and its parents when they don't exist.
    /// </summary>
    /// <param name="folder">The path of the folder on the server, with <c>/</c> separators.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the folder exists.</returns>
    Task EnsureFolderAsync(string folder, CancellationToken cancellationToken);

    /// <summary>
    /// Tells whether a file exists on the server.
    /// </summary>
    /// <param name="remotePath">The path of the file on the server.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> when the file exists.</returns>
    Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken);

    /// <summary>
    /// Uploads a file.
    /// </summary>
    /// <param name="content">The content of the file.</param>
    /// <param name="remotePath">The path of the file on the server.</param>
    /// <param name="overwrite">Whether to replace the file when it exists; otherwise the upload fails.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the file is uploaded.</returns>
    Task UploadAsync(Stream content, string remotePath, bool overwrite, CancellationToken cancellationToken);
}
