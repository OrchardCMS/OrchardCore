namespace OrchardCore.DataPipelines.Ftp;

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
