using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Documents;

namespace OrchardCore.Deployment.Remote.Services;

public class RemoteInstanceService
{
    private readonly IDocumentManager<RemoteInstanceList> _documentManager;

    public RemoteInstanceService(IDocumentManager<RemoteInstanceList> documentManager) => _documentManager = documentManager;

    /// <summary>
    /// Loads the remote instances document from the store for updating and that should not be cached.
    /// </summary>
    public Task<RemoteInstanceList> LoadRemoteInstanceListAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the remote instances document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<RemoteInstanceList> GetRemoteInstanceListAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task<RemoteInstance> LoadRemoteInstanceAsync(string id)
    {
        var remoteInstanceList = await LoadRemoteInstanceListAsync().ConfigureAwait(false);
        return FindRemoteInstance(remoteInstanceList, id);
    }

    public async Task<RemoteInstance> GetRemoteInstanceAsync(string id)
    {
        var remoteInstanceList = await GetRemoteInstanceListAsync().ConfigureAwait(false);
        return FindRemoteInstance(remoteInstanceList, id);
    }

    public async Task DeleteRemoteInstanceAsync(string id)
    {
        var remoteInstanceList = await LoadRemoteInstanceListAsync().ConfigureAwait(false);
        var remoteInstance = FindRemoteInstance(remoteInstanceList, id);

        if (remoteInstance != null)
        {
            remoteInstanceList.RemoteInstances.Remove(remoteInstance);
            await _documentManager.UpdateAsync(remoteInstanceList).ConfigureAwait(false);
        }
    }

    public async Task CreateRemoteInstanceAsync(string name, string url, string clientName, string apiKey)
    {
        var remoteInstanceList = await LoadRemoteInstanceListAsync().ConfigureAwait(false);

        remoteInstanceList.RemoteInstances.Add(new RemoteInstance
        {
            Id = Guid.NewGuid().ToString("n"),
            Name = name,
            Url = url,
            ClientName = clientName,
            ApiKey = apiKey,
        });

        await _documentManager.UpdateAsync(remoteInstanceList).ConfigureAwait(false);
    }

    public async Task UpdateRemoteInstance(string id, string name, string url, string clientName, string apiKey)
    {
        var remoteInstanceList = await LoadRemoteInstanceListAsync().ConfigureAwait(false);
        var remoteInstance = FindRemoteInstance(remoteInstanceList, id);

        remoteInstance.Name = name;
        remoteInstance.Url = url;
        remoteInstance.ClientName = clientName;
        remoteInstance.ApiKey = apiKey;

        await _documentManager.UpdateAsync(remoteInstanceList).ConfigureAwait(false);
    }

    private static RemoteInstance FindRemoteInstance(RemoteInstanceList remoteInstanceList, string id) =>
        remoteInstanceList.RemoteInstances.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
}
