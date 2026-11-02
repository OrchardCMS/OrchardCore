namespace OrchardCore.DataPipelines.Sftp;

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
