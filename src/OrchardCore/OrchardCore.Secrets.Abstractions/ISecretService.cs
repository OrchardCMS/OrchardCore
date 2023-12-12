using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    SecretBase CreateSecret(string typeName);
    TSecret CreateSecret<TSecret>() where TSecret : SecretBase, new();
    Task<SecretBase> GetSecretAsync(string name);
    Task<TSecret> GetSecretAsync<TSecret>(string name) where TSecret : SecretBase, new();

    Task<TSecret> GetOrCreateSecretAsync<TSecret>(string name, Action<TSecret> configure = null, string sourceName = null)
        where TSecret : SecretBase, new();

    Task RemoveSecretAsync(string name);
    Task<bool> TryRemoveSecretAsync(string name);

    Task<IDictionary<string, SecretInfo>> GetSecretInfosAsync();
    Task<IDictionary<string, SecretInfo>> LoadSecretInfosAsync();
    IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos();
    Task UpdateSecretAsync(SecretBase secret);
    Task UpdateSecretAsync(SecretInfo secretInfo, SecretBase secret, string sourceName = null);
}
