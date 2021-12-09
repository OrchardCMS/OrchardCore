using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Deployment.Remote.Models;
using YesSql;

namespace OrchardCore.Deployment.Remote.Services
{
    public class RemoteClientService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ISession _session;

        private RemoteClientList _remoteClientList;

        public RemoteClientService(
            ISession session,
            IDataProtectionProvider dataProtectionProvider
            )
        {
            _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Deployment").ToTimeLimitedDataProtector();
            _session = session;
        }

        public async Task<RemoteClientList> GetRemoteClientListAsync()
        {
            if (_remoteClientList != null)
            {
                return _remoteClientList;
            }

            _remoteClientList = await _session.Query<RemoteClientList>().FirstOrDefaultAsync();

            if (_remoteClientList == null)
            {
                _remoteClientList = new RemoteClientList();
                _session.Save(_remoteClientList);
            }

            return _remoteClientList;
        }

        public async Task<RemoteClient> GetRemoteClientAsync(string id)
        {
            var remoteClientList = await GetRemoteClientListAsync();
            return remoteClientList.RemoteClients.FirstOrDefault(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
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
                ProtectedApiKey = _dataProtector.Protect(Encoding.UTF8.GetBytes(apiKey)),
            };

            remoteClientList.RemoteClients.Add(remoteClient);
            _session.Save(remoteClientList);

            return remoteClient;
        }

        public async Task<bool> TryUpdateRemoteClient(string id, string clientName, string apiKey)
        {
            var remoteClient = await GetRemoteClientAsync(id);

            if (remoteClient == null)
            {
                return false;
            }

            remoteClient.ClientName = clientName;
            remoteClient.ProtectedApiKey = _dataProtector.Protect(Encoding.UTF8.GetBytes(apiKey));

            _session.Save(_remoteClientList);

            return true;
        }
    }
}
