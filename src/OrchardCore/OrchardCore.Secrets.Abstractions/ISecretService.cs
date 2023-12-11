using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    SecretBase CreateSecret(string typeName);
    Task<SecretBase> GetSecretAsync(string name);
    TSecret CreateSecret<TSecret>() where TSecret : SecretBase, new();
    Task<TSecret> GetSecretAsync<TSecret>(string name) where TSecret : SecretBase, new();

    Task<TSecret> GetOrCreateSecretAsync<TSecret>(string name, Action<TSecret> configure = null, string sourceName = null)
        where TSecret : SecretBase, new();

    Task UpdateSecretAsync(SecretBase secret);
    Task RemoveSecretAsync(string name);

    Task<SecretBase> GetSecretAsync(SecretBinding binding);
    Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
    Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
    IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos();
    Task UpdateSecretAsync(SecretBinding binding, SecretBase secret, string sourceName = null);
    Task RemoveSecretAsync(SecretBinding binding);
}
