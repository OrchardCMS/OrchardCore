using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.KeyVault.Services;

public class KeyVaultSecretStore : ISecretStore
{
    private readonly KeyVaultClientService _keyVaultClientService;
    protected readonly IStringLocalizer S;

    public KeyVaultSecretStore(
        KeyVaultClientService keyVaultClientService,
        IStringLocalizer<KeyVaultSecretStore> stringLocalizer)
    {
        _keyVaultClientService = keyVaultClientService;
        S = stringLocalizer;

    }

    public string Name => nameof(KeyVaultSecretStore);
    public string DisplayName => S["KeyVault Secrets Store"];
    public bool IsReadOnly => false;

    public async Task<SecretBase> GetSecretAsync(string key, Type type)
    {
        if (!typeof(SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException("The type must implement " + nameof(SecretBase));
        }

        var value = await _keyVaultClientService.GetSecretAsync(key);
        if (!String.IsNullOrEmpty(value))
        {
            return JsonConvert.DeserializeObject(value, type) as SecretBase;
        }

        return null;
    }

    public Task UpdateSecretAsync(string key, SecretBase secret)
    {
        var value = JsonConvert.SerializeObject(secret);

        return _keyVaultClientService.SetSecretAsync(key, value);
    }

    public Task RemoveSecretAsync(string key) => _keyVaultClientService.RemoveSecretAsync(key);
}
