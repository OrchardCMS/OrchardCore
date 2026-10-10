using System.Text;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Creates SFTP clients with SSH.NET.
/// </summary>
public sealed class SshNetSftpClientFactory : IDataPipelineSftpClientFactory
{
    public IDataPipelineFileTransferClient CreateClient(DataPipelineSftpConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return new SshNetSftpClient(connection);
    }

    private sealed class SshNetSftpClient : IDataPipelineFileTransferClient
    {
        private readonly DataPipelineSftpConnection _connection;
        private readonly List<IDisposable> _disposables = [];
        private SftpClient _client;

        public SshNetSftpClient(DataPipelineSftpConnection connection)
        {
            _connection = connection;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var methods = new List<AuthenticationMethod>();

            if (!string.IsNullOrEmpty(_connection.PrivateKey))
            {
                PrivateKeyFile keyFile;

                try
                {
                    using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(_connection.PrivateKey.Trim() + "\n"));
                    keyFile = string.IsNullOrEmpty(_connection.Passphrase)
                        ? new PrivateKeyFile(keyStream)
                        : new PrivateKeyFile(keyStream, _connection.Passphrase);
                }
                catch (SshException ex)
                {
                    // The message of SSH.NET doesn't hold the key; it tells whether the key or its passphrase is wrong.
                    throw new InvalidOperationException($"The private key can't be read: {ex.Message}", ex);
                }

                if (keyFile is IDisposable disposableKey)
                {
                    _disposables.Add(disposableKey);
                }

                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, keyFile));
            }

            if (!string.IsNullOrEmpty(_connection.Password))
            {
                var passwordMethod = new PasswordAuthenticationMethod(_connection.Username, _connection.Password);

                if (passwordMethod is IDisposable disposablePassword)
                {
                    _disposables.Add(disposablePassword);
                }

                methods.Add(passwordMethod);
            }

            var connectionInfo = new ConnectionInfo(_connection.Host, _connection.Port, _connection.Username, [.. methods])
            {
                Timeout = _connection.Timeout,
            };

            if (connectionInfo is IDisposable disposableConnectionInfo)
            {
                _disposables.Add(disposableConnectionInfo);
            }

            _client = new SftpClient(connectionInfo)
            {
                OperationTimeout = _connection.Timeout,
            };

            _client.HostKeyReceived += (_, e) => e.CanTrust = _connection.ValidateHostKey?.Invoke(e.FingerPrintSHA256) ?? true;

            await _client.ConnectAsync(cancellationToken);
        }

        public async Task EnsureFolderAsync(string folder, CancellationToken cancellationToken)
        {
            var path = folder.StartsWith('/') ? "/" : string.Empty;

            foreach (var segment in folder.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                path = path.Length == 0 || path.EndsWith('/') ? path + segment : path + "/" + segment;

                if (!await _client.ExistsAsync(path, cancellationToken))
                {
                    await _client.CreateDirectoryAsync(path, cancellationToken);
                }
            }
        }

        public Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken)
            => _client.ExistsAsync(remotePath, cancellationToken);

        public Task UploadAsync(Stream content, string remotePath, bool overwrite, CancellationToken cancellationToken)
            => _client.UploadFileAsync(content, remotePath, overwrite, (IProgress<UploadFileProgressReport>)null, cancellationToken);

        public ValueTask DisposeAsync()
        {
            if (_client is not null)
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        _client.Disconnect();
                    }
                }
                catch (Exception ex) when (ex is SshException or IOException or ObjectDisposedException)
                {
                    // The files are uploaded: a failure to say goodbye doesn't matter.
                }
                finally
                {
                    _client.Dispose();
                }
            }

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }
}
