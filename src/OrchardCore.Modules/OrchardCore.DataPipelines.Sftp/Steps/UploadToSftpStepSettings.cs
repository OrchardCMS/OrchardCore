using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Sftp;

/// <summary>
/// The settings of an <see cref="UploadToSftpStep"/>.
/// </summary>
public sealed class UploadToSftpStepSettings
{
    /// <summary>
    /// The number of seconds to wait for the server, by default.
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;

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
    /// Gets or sets the password, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedPassword { get; set; }

    /// <summary>
    /// Gets or sets the private key, in PEM or OpenSSH format, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedPrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the passphrase of the private key, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedPassphrase { get; set; }

    /// <summary>
    /// Gets or sets the expected SHA-256 fingerprint of the host key, such as <c>SHA256:ohD8VZEXGWo6Ez8GSEJQ9Wpaf...</c>.
    /// When it is set, the step refuses to connect to a server with another key. When it is empty, the host is not
    /// verified.
    /// </summary>
    public string HostKeyFingerprint { get; set; }

    /// <summary>
    /// Gets or sets the folder the files are uploaded to. It can use the placeholders of
    /// <see cref="DataPipelineTemplate"/>, and is created when it doesn't exist. When it is empty, the files are
    /// uploaded to the home folder of the user.
    /// </summary>
    public string RemoteFolder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether existing files are replaced; otherwise the step fails when a file exists.
    /// </summary>
    public bool Overwrite { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of seconds to wait for the server.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;
}
