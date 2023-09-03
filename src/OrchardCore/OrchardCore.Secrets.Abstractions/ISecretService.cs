using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    public SecretBase CreateSecret(string typeName);
    Task<SecretBase> GetSecretAsync(string name, Type type);
    Task<SecretBase> GetSecretAsync(SecretBinding binding);
    Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
    Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
    IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos();
    Task UpdateSecretAsync(string name, SecretBinding binding, SecretBase secret);
    Task RemoveSecretAsync(string name, string storeName);
}
