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

            await _secretService.RemoveSecretAsync($"{Secrets.Encryption}.{remoteClient.ClientName}");
            await _secretService.RemoveSecretAsync($"{Secrets.Signing}.{remoteClient.ClientName}");
            await _secretService.RemoveSecretAsync($"{Secrets.ApiKey}.{remoteClient.ClientName}");

            remoteClientList.RemoteClients.Remove(remoteClient);
            await _session.SaveAsync(remoteClientList);
        }

        public async Task<RemoteClient> CreateRemoteClientAsync(string clientName, string apiKey)
        {
            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: $"{Secrets.Encryption}.{clientName}",
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: $"{Secrets.Signing}.{clientName}",
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.Public));

            await _secretService.GetOrCreateSecretAsync<TextSecret>(
                name: $"{Secrets.ApiKey}.{clientName}",
                configure: secret => secret.Text = apiKey);

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

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: $"{Secrets.Encryption}.{clientName}",
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate),
                sourceName: $"{Secrets.Encryption}.{remoteClient.ClientName}");

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: $"{Secrets.Signing}.{clientName}",
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.Public),
                sourceName: $"{Secrets.Signing}.{remoteClient.ClientName}");

            var apiKeySecret = await _secretService.GetOrCreateSecretAsync<TextSecret>(
                name: $"{Secrets.ApiKey}.{clientName}",
                configure: secret => secret.Text = apiKey,
                sourceName: $"{Secrets.ApiKey}.{remoteClient.ClientName}");

            if (apiKey is not null && apiKeySecret.Text != apiKey)
            {
                apiKeySecret.Text = apiKey;
                await _secretService.UpdateSecretAsync(apiKeySecret);
            }

            remoteClient.ClientName = clientName;

            await _session.SaveAsync(_remoteClientList);
        }
    }
}
