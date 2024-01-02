using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
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
                await _session.SaveAsync(_remoteClientList);
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
            if (remoteClient is null)
            {
                return;
            }

            await _secretService.RemoveSecretAsync($"{RemoteSecret.Namespace}.{remoteClient.ClientName}.Encryption");
            await _secretService.RemoveSecretAsync($"{RemoteSecret.Namespace}.{remoteClient.ClientName}.Signing");
            await _secretService.RemoveSecretAsync($"{RemoteSecret.Namespace}.{remoteClient.ClientName}.ApiKey");

            remoteClientList.RemoteClients.Remove(remoteClient);
            await _session.SaveAsync(remoteClientList);
        }

        public async Task<RemoteClient> CreateRemoteClientAsync(string clientName, string apiKey)
        {
            await _secretService.GetOrAddSecretAsync<RSASecret>(
                $"{RemoteSecret.Namespace}.{clientName}.Encryption",
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Remote Client Secret holding a raw RSA key to be used for decryption.";
                });

            await _secretService.GetOrAddSecretAsync<RSASecret>(
                $"{RemoteSecret.Namespace}.{clientName}.Signing",
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.Public);
                    info.Description = "Remote Client Secret holding a raw RSA key to verify the signature.";
                });

            await _secretService.GetOrAddSecretAsync<TextSecret>(
                $"{RemoteSecret.Namespace}.{clientName}.ApiKey",
                (secret, info) =>
                {
                    secret.Text = apiKey;
                    info.Description = "Remote Client Secret holding an Api Key.";
                });

            var remoteClientList = await GetRemoteClientListAsync();

            var remoteClient = new RemoteClient
            {
                Id = Guid.NewGuid().ToString("n"),
                ClientName = clientName,
            };

            remoteClientList.RemoteClients.Add(remoteClient);
            await _session.SaveAsync(remoteClientList);

            return remoteClient;
        }

        public Task UpdateRemoteClientAsync(RemoteClient remoteClient, string apiKey)
            => UpdateRemoteClientAsync(remoteClient.Id, remoteClient.ClientName, apiKey);

        public async Task UpdateRemoteClientAsync(string id, string clientName, string apiKey)
        {
            var remoteClient = await GetRemoteClientAsync(id);
            if (remoteClient is null)
            {
                return;
            }

            await _secretService.GetOrAddSecretAsync<RSASecret>(
                $"{RemoteSecret.Namespace}.{clientName}.Encryption",
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Remote Client Secret holding a raw RSA key to be used for encryption.";
                },
                source: $"{RemoteSecret.Namespace}.{remoteClient.ClientName}.Encryption");

            await _secretService.GetOrAddSecretAsync<RSASecret>(
                $"{RemoteSecret.Namespace}.{clientName}.Signing",
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.Public);
                    info.Description = "Remote Client Secret holding a raw RSA key to be used for signing.";
                },
                source: $"{RemoteSecret.Namespace}.{remoteClient.ClientName}.Signing");

            var apiKeySecret = await _secretService.GetOrAddSecretAsync<TextSecret>(
                $"{RemoteSecret.Namespace}.{clientName}.ApiKey",
                (secret, info) =>
                {
                    secret.Text = apiKey;
                    info.Description = "Remote Client Secret holding an Api Key.";
                },
                source: $"{RemoteSecret.Namespace}.{remoteClient.ClientName}.ApiKey");

            if (apiKeySecret.Text != apiKey)
            {
                apiKeySecret.Text = apiKey;
                await _secretService.UpdateSecretAsync(apiKeySecret);
            }

            remoteClient.ClientName = clientName;

            await _session.SaveAsync(_remoteClientList);
        }
    }
}
