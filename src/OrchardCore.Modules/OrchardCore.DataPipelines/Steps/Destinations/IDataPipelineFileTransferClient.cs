namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A connection to a file server, such as an FTP or SFTP server, that the upload steps send files to. Create one with
/// <see cref="IDataPipelineFtpClientFactory"/> or <see cref="IDataPipelineSftpClientFactory"/>, connect it, then upload
/// the files. Disposing it closes the connection.
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

/// <summary>
/// Creates the clients the <see cref="UploadToFtpStep"/> uploads files with.
/// </summary>
public interface IDataPipelineFtpClientFactory
{
    /// <summary>
    /// Creates a client for an FTP server. The client is not connected yet.
    /// </summary>
    /// <param name="connection">The server and the credentials to connect with.</param>
    /// <returns>The client.</returns>
    IDataPipelineFileTransferClient CreateClient(DataPipelineFtpConnection connection);
}

/// <summary>
/// Creates the clients the <see cref="UploadToSftpStep"/> uploads files with.
/// </summary>
public interface IDataPipelineSftpClientFactory
{
    /// <summary>
    /// Creates a client for an SFTP server. The client is not connected yet.
    /// </summary>
    /// <param name="connection">The server and the credentials to connect with.</param>
    /// <returns>The client.</returns>
    IDataPipelineFileTransferClient CreateClient(DataPipelineSftpConnection connection);
}

/// <summary>
/// How to connect to an FTP server.
/// </summary>
public sealed class DataPipelineFtpConnection
{
    /// <summary>
    /// Gets or sets the host name or IP address of the server.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port of the server.
    /// </summary>
    public int Port { get; set; } = 21;

    /// <summary>
    /// Gets or sets how the connection is encrypted.
    /// </summary>
    public DataPipelineFtpEncryption Encryption { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the certificate of the server must be valid.
    /// </summary>
    public bool ValidateCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password, in clear text.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets how long to wait for the server.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// How to connect to an SFTP server.
/// </summary>
public sealed class DataPipelineSftpConnection
{
    /// <summary>
    /// Gets or sets the host name or IP address of the server.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port of the server.
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password, in clear text, or <see langword="null"/> to authenticate with a private key only.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the private key, in PEM or OpenSSH format, or <see langword="null"/> to authenticate with a password
    /// only.
    /// </summary>
    public string PrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the passphrase of the private key, or <see langword="null"/> when it has none.
    /// </summary>
    public string Passphrase { get; set; }

    /// <summary>
    /// Gets or sets how long to wait for the server.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the delegate that decides whether to trust the server. It receives the SHA-256 fingerprint of the
    /// host key, as unpadded Base64 without the <c>SHA256:</c> prefix, before the client authenticates, and returns
    /// <see langword="false"/> to refuse the connection. When it is <see langword="null"/>, every host is trusted.
    /// </summary>
    public Func<string, bool> ValidateHostKey { get; set; }
}

/// <summary>
/// How the connection to an FTP server is encrypted.
/// </summary>
public enum DataPipelineFtpEncryption
{
    /// <summary>
    /// The client connects in clear text, then switches to TLS with the <c>AUTH TLS</c> command (FTPES). Most servers
    /// support it on port 21.
    /// </summary>
    ExplicitTls,

    /// <summary>
    /// The connection is encrypted from the start (FTPS), usually on port 990.
    /// </summary>
    ImplicitTls,

    /// <summary>
    /// The connection is not encrypted: the credentials and the files travel in clear text.
    /// </summary>
    None,
}
