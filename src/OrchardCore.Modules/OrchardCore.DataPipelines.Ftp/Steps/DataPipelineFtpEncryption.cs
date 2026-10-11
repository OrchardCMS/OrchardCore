namespace OrchardCore.DataPipelines.Ftp;

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
