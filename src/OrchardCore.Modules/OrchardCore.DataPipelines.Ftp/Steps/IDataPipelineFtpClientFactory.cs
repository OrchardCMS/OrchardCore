using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Ftp;

/// <summary>
/// Creates the clients the <see cref="UploadToFtpStep"/> uploads files with. The default implementation uses
/// FluentFTP; register another one to replace it.
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
