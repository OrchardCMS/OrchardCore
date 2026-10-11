using System.Net.Security;
using FluentFTP;
using FluentFTP.Exceptions;
using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Ftp;

/// <summary>
/// Creates FTP clients with FluentFTP.
/// </summary>
public sealed class FluentFtpClientFactory : IDataPipelineFtpClientFactory
{
    /// <inheritdoc/>
    public IDataPipelineFileTransferClient CreateClient(DataPipelineFtpConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return new FluentFtpClient(connection);
    }

    private sealed class FluentFtpClient : IDataPipelineFileTransferClient
    {
        private readonly AsyncFtpClient _client;

        public FluentFtpClient(DataPipelineFtpConnection connection)
        {
            var timeout = (int)Math.Min(int.MaxValue, connection.Timeout.TotalMilliseconds);

            var config = new FtpConfig
            {
                EncryptionMode = connection.Encryption switch
                {
                    DataPipelineFtpEncryption.ImplicitTls => FtpEncryptionMode.Implicit,
                    DataPipelineFtpEncryption.None => FtpEncryptionMode.None,
                    _ => FtpEncryptionMode.Explicit,
                },
                ValidateAnyCertificate = !connection.ValidateCertificate,
                ConnectTimeout = timeout,
                ReadTimeout = timeout,
                DataConnectionConnectTimeout = timeout,
                DataConnectionReadTimeout = timeout,
            };

            _client = new AsyncFtpClient(connection.Host, connection.Username ?? string.Empty, connection.Password ?? string.Empty, connection.Port, config);

            if (connection.ValidateCertificate)
            {
                _client.ValidateCertificate += (_, e) => e.Accept = e.PolicyErrors == SslPolicyErrors.None;
            }
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
            => _client.Connect(cancellationToken);

        public async Task EnsureFolderAsync(string folder, CancellationToken cancellationToken)
        {
            if (!await _client.DirectoryExists(folder, cancellationToken))
            {
                await _client.CreateDirectory(folder, force: true, cancellationToken);
            }
        }

        public Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken)
            => _client.FileExists(remotePath, cancellationToken);

        public async Task UploadAsync(Stream content, string remotePath, bool overwrite, CancellationToken cancellationToken)
        {
            var status = await _client.UploadStream(
                content,
                remotePath,
                overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                createRemoteDir: false,
                progress: null,
                cancellationToken);

            if (status == FtpStatus.Skipped)
            {
                throw new IOException($"The file '{remotePath}' already exists.");
            }

            if (status == FtpStatus.Failed)
            {
                throw new IOException($"The server didn't accept the file '{remotePath}'.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_client.IsConnected)
                {
                    await _client.Disconnect(CancellationToken.None);
                }
            }
            catch (Exception ex) when (ex is IOException or TimeoutException or FtpException)
            {
                // The files are uploaded: a failure to say goodbye doesn't matter.
            }
            finally
            {
                await _client.DisposeAsync();
            }
        }
    }
}
