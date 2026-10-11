using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Ftp;

/// <summary>
/// The settings of an <see cref="UploadToFtpStep"/>.
/// </summary>
public sealed class UploadToFtpStepSettings
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
    public int Port { get; set; } = 21;

    /// <summary>
    /// Gets or sets how the connection is encrypted.
    /// </summary>
    public DataPipelineFtpEncryption Encryption { get; set; } = DataPipelineFtpEncryption.ExplicitTls;

    /// <summary>
    /// Gets or sets a value indicating whether the certificate of the server must be valid.
    /// </summary>
    public bool ValidateCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedPassword { get; set; }

    /// <summary>
    /// Gets or sets the folder the files are uploaded to. It can use the placeholders of
    /// <see cref="DataPipelineTemplate"/>, and is created when it doesn't exist. When it is empty, the files are
    /// uploaded to the folder the user starts in.
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
