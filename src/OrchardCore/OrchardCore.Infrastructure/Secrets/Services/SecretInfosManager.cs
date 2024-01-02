using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretInfosManager
{
    private readonly IDocumentManager<SecretInfosDocument> _documentManager;

    public SecretInfosManager(IDocumentManager<SecretInfosDocument> documentManager) => _documentManager = documentManager;

    /// <summary>
    /// Returns the document from the database to be updated.
    /// </summary>
    public Task<SecretInfosDocument> LoadSecretInfosAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Returns the document from the cache or creates a new one. The result should not be updated.
    /// </summary>
    public Task<SecretInfosDocument> GetSecretInfosAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task RemoveSecretInfoAsync(string name)
    {
        var document = await LoadSecretInfosAsync();
        document.SecretInfos.Remove(name);
        await _documentManager.UpdateAsync(document);
    }

    public async Task UpdateSecretInfoAsync(string name, SecretInfo secretInfo)
    {
        var document = await LoadSecretInfosAsync();
        document.SecretInfos[name] = secretInfo;
        await _documentManager.UpdateAsync(document);
    }
}
