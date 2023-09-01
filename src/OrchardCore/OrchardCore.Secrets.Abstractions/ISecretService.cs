using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    public SecretBase CreateSecret(string typeName);
    Task<SecretBase> GetSecretAsync(string key, Type type);
    Task<SecretBase> GetSecretAsync(SecretBinding binding);
    Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
    Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
    IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos();
    Task UpdateSecretAsync(string key, SecretBinding secretBinding, SecretBase secret);
    Task RemoveSecretAsync(string key, string storeName);
}
