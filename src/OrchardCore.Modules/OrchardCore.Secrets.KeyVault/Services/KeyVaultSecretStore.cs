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

    public async Task<SecretBase> GetSecretAsync(string name, Type type)
    {
        if (!typeof(SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException("The type must implement " + nameof(SecretBase));
        }

        var value = await _keyVaultClientService.GetSecretAsync(name);
        if (String.IsNullOrEmpty(value))
        {
            return null;
        }

        var secret = JsonConvert.DeserializeObject(value, type) as SecretBase;
        if (secret is not null)
        {
            secret.Name = name;
        }

        return secret;
    }

    public Task UpdateSecretAsync(string name, SecretBase secret)
    {
        var value = JsonConvert.SerializeObject(secret);

        return _keyVaultClientService.SetSecretAsync(name, value);
    }

    public Task RemoveSecretAsync(string name) => _keyVaultClientService.RemoveSecretAsync(name);
}
