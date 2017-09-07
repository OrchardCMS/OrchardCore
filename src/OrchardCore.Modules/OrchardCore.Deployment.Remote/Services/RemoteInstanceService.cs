using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment.Remote.Models;
using YesSql;

namespace OrchardCore.Deployment.Remote.Services
{
    public class RemoteInstanceService
    {
        private readonly ISession _session;
        private RemoteInstanceList _remoteInstanceList;

        public RemoteInstanceService(ISession session)
        {
            _session = session;
        }

        public async Task<RemoteInstanceList> GetRemoteInstanceListAsync()
        {
            if (_remoteInstanceList != null)
            {
                return _remoteInstanceList;
            }

            _remoteInstanceList = await _session.Query<RemoteInstanceList>().FirstOrDefaultAsync();

            if (_remoteInstanceList == null)
            {
                _remoteInstanceList = new RemoteInstanceList();
                _session.Save(_remoteInstanceList);
            }

            return _remoteInstanceList;
        }

        public async Task<RemoteInstance> GetRemoteInstanceAsync(string id)
        {
            var remoteInstanceList = await GetRemoteInstanceListAsync();
            return remoteInstanceList.RemoteInstances.FirstOrDefault(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task DeleteRemoteInstanceAsync(string id)
        {
            var remoteInstanceList = await GetRemoteInstanceListAsync();
            var remoteInstance = await GetRemoteInstanceAsync(id);

            if (remoteInstance != null)
            {
                remoteInstanceList.RemoteInstances.Remove(remoteInstance);
                _session.Save(remoteInstanceList);
            }
        }

        public async Task<RemoteInstance> CreateRemoteInstanceAsync(string name, string url, string clientName, string apiKey)
        {
            var remoteInstanceList = await GetRemoteInstanceListAsync();

            var remoteInstance = new RemoteInstance
            {
                Id = Guid.NewGuid().ToString("n"),
                Name = name,
                Url = url,
                ClientName = clientName,
                ApiKey = apiKey,
            };

            remoteInstanceList.RemoteInstances.Add(remoteInstance);
            _session.Save(remoteInstanceList);

            return remoteInstance;
        }

        public async Task<bool> TryUpdateRemoteInstance(string id, string name, string url, string clientName, string apiKey)
        {
            var remoteInstanceList = await GetRemoteInstanceListAsync();
            var remoteInstance = await GetRemoteInstanceAsync(id);

            if (remoteInstance == null)
            {
                return false;
            }

            remoteInstance.Name = name;
            remoteInstance.Url = url;
            remoteInstance.ClientName = clientName;
            remoteInstance.ApiKey = apiKey;

            _session.Save(_remoteInstanceList);

            return true;
        }
    }
}
