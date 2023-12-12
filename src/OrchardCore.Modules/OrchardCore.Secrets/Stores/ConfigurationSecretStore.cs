using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Stores;

public class ConfigurationSecretStore : ISecretStore
{
    private readonly IConfigurationSection _configuration;
    protected readonly IStringLocalizer S;

    public ConfigurationSecretStore(
        IShellConfiguration shellConfiguration,
        IStringLocalizer<ConfigurationSecretStore> stringLocalizer)
    {
        _configuration = shellConfiguration.GetSection("OrchardCore_Secrets:Secrets");
        S = stringLocalizer;
    }

    public string Name => nameof(ConfigurationSecretStore);
    public string DisplayName => S["Configuration Secret Store"];
    public bool IsReadOnly => true;

    public Task<SecretBase> GetSecretAsync(string name, Type type)
    {
        if (!typeof(SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement '{nameof(SecretBase)}'.");
        }

        if (!_configuration.Exists())
        {
            return Task.FromResult<SecretBase>(null);
        }

        var section = _configuration.GetSection(name);
        if (!section.Exists())
        {
            return Task.FromResult<SecretBase>(null);
        }

        var secret = section.Get(type) as SecretBase;
        return Task.FromResult(secret);
    }

    public Task UpdateSecretAsync(string name, SecretBase secret) =>
        throw new NotSupportedException("The Configuration Secret Store is 'ReadOnly'.");

    public Task RemoveSecretAsync(string name) =>
        throw new NotSupportedException("The Configuration Secret Store is 'ReadOnly'.");
}
