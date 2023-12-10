using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;
using YesSql;

namespace OrchardCore.Deployment.Remote.Services
{
    public class RemoteClientService
    {
        private readonly ISecretService _secretService;
        private readonly ISession _session;

        private RemoteClientList _remoteClientList;

        public RemoteClientService(ISecretService secretService, ISession session)
        {
            _secretService = secretService;
            _session = session;
        }

        public async Task<RemoteClientList> GetRemoteClientListAsync()
        {
            if (_remoteClientList != null)
            {
                return _remoteClientList;
            }

            _remoteClientList = await _session.Query<RemoteClientList>().FirstOrDefaultAsync();
            if (_remoteClientList is null)
            {
                _remoteClientList = new RemoteClientList();
                _session.Save(_remoteClientList);
            }

            return _remoteClientList;
        }

        public async Task<RemoteClient> GetRemoteClientAsync(string id)
        {
            var remoteClientList = await GetRemoteClientListAsync();
            return remoteClientList.RemoteClients.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task DeleteRemoteClientAsync(string id)
        {
            var remoteClientList = await GetRemoteClientListAsync();
            var remoteClient = await GetRemoteClientAsync(id);

            if (remoteClient != null)
            {
                remoteClientList.RemoteClients.Remove(remoteClient);
                _session.Save(remoteClientList);
            }
        }

        public async Task<RemoteClient> CreateRemoteClientAsync(string clientName, string apiKey)
        {
            var remoteClientList = await GetRemoteClientListAsync();

            var remoteClient = new RemoteClient
            {
                Id = Guid.NewGuid().ToString("n"),
                ClientName = clientName,
            };

            var rsaEncryptionSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaEncryptionSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                    secret.KeyType = RSAKeyType.PublicPrivatePair;
                });

            var rsaSigningSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaSigningSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.KeyType = RSAKeyType.Public;
                });

            var secret = await _secretService.GetOrCreateSecretAsync<TextSecret>(
                $"OrchardCore.Deployment.Remote.ApiKey.{clientName}",
                secret =>
                {
                    secret.Text = apiKey;
                });

            if (secret.Text != apiKey)
            {
                secret.Text = apiKey;
                await _secretService.UpdateSecretAsync(secret);
            }

            remoteClientList.RemoteClients.Add(remoteClient);
            _session.Save(remoteClientList);

            return remoteClient;
        }

        public async Task<bool> TryUpdateRemoteClientAsync(string id, string clientName, string apiKey)
        {
            var remoteClient = await GetRemoteClientAsync(id);
            if (remoteClient is null)
            {
                return false;
            }

            remoteClient.ClientName = clientName;

            var rsaEncryptionSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaEncryptionSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                    secret.KeyType = RSAKeyType.PublicPrivatePair;
                });

            var rsaSigningSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaSigningSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.KeyType = RSAKeyType.Public;
                });

            var secret = await _secretService.GetOrCreateSecretAsync<TextSecret>(
                $"OrchardCore.Deployment.Remote.ApiKey.{remoteClient.ClientName}",
                secret =>
                {
                    secret.Text = apiKey;
                });

            if (secret.Text != apiKey)
            {
                secret.Text = apiKey;
                await _secretService.UpdateSecretAsync(secret);
            }

            _session.Save(_remoteClientList);

            return true;
        }
    }
}
