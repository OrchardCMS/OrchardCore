using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    SecretBase CreateSecret(string typeName);
    Task<SecretBase> GetSecretAsync(string name);
    Task<IDictionary<string, SecretInfo>> GetSecretInfosAsync();
    Task<IDictionary<string, SecretInfo>> LoadSecretInfosAsync();
    IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos();
    Task UpdateSecretAsync(SecretBase secret, SecretInfo info = null, string source = null);
    Task<bool> RemoveSecretAsync(string name);
}
