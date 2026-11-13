using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Sftp;

/// <summary>
/// Creates the clients the <see cref="UploadToSftpStep"/> uploads files with. The default implementation uses
/// SSH.NET; register another one to replace it.
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
