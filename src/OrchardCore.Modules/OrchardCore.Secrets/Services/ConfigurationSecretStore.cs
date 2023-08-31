using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class ConfigurationSecretStore : ISecretStore
{
    private readonly IShellConfiguration _shellConfiguration;
    protected readonly IStringLocalizer S;

    public ConfigurationSecretStore(
        IShellConfiguration shellConfiguration,
        IStringLocalizer<ConfigurationSecretStore> stringLocalizer)
    {
        _shellConfiguration = shellConfiguration;
        S = stringLocalizer;
    }

    public string Name => nameof(ConfigurationSecretStore);
    public string DisplayName => S["Configuration Secret Store"];
    public bool IsReadOnly => true;

    public Task<SecretBase> GetSecretAsync(string key, Type type)
    {
        if (!typeof(SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException("The type must implement " + nameof(SecretBase));
        }

        var secret = _shellConfiguration.GetSection($"OrchardCore_Secrets_ConfigurationSecretStore:{key}").Get(type) as SecretBase;
        if (secret is not null)
        {
            secret.Name = key;
        }

        return Task.FromResult(secret);
    }

    public Task UpdateSecretAsync(string key, SecretBase secret) =>
        throw new NotSupportedException("The Configuration Secret Store is ReadOnly");

    public Task RemoveSecretAsync(string key) =>
        throw new NotSupportedException("The Configuration Secret Store is ReadOnly");
}
