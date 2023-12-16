using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;
using OrchardCore.Secrets.Stores;

namespace OrchardCore.Secrets.Services;

public class SecretService : ISecretService
{
    private readonly SecretInfosManager _secretInfosManager;
    private readonly IReadOnlyCollection<SecretStoreInfo> _storeInfos;
    private readonly IEnumerable<ISecretStore> _stores;

    private readonly Dictionary<string, SecretActivator> _activators = new();

    public SecretService(SecretInfosManager secretInfosManager, IEnumerable<ISecretStore> stores, IOptions<SecretOptions> options)
    {
        _secretInfosManager = secretInfosManager;

        _storeInfos = stores.Select(store => new SecretStoreInfo
        {
            Name = store.Name,
            IsReadOnly = store.IsReadOnly,
            DisplayName = store.DisplayName,
        })
        .ToArray();

        _stores = stores;

        foreach (var type in options.Value.SecretTypes)
        {
            var activatorType = typeof(SecretActivator<>).MakeGenericType(type);
            var activator = (SecretActivator)Activator.CreateInstance(activatorType);
            _activators[type.Name] = activator;
        }
    }

    public SecretBase CreateSecret(string typeName)
    {
        if (!_activators.TryGetValue(typeName, out var factory) || !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            throw new ArgumentException($"The type should be configured and should implement '{nameof(SecretBase)}'.", nameof(typeName));
        }

        return factory.Create();
    }

    public async Task<SecretBase> GetSecretAsync(string name)
    {
        var secretInfos = await GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(name, out var secretInfo))
        {
            return null;
        }

        return await GetSecretAsync(secretInfo);
    }

    public TSecret CreateSecret<TSecret>() where TSecret : SecretBase, new() => CreateSecret(typeof(TSecret).Name) as TSecret;

    public async Task<TSecret> GetSecretAsync<TSecret>(string name) where TSecret : SecretBase, new()
        => await GetSecretAsync(name) as TSecret;

    public async Task<TSecret> GetOrCreateSecretAsync<TSecret>(string name, Action<TSecret> configure = null, string sourceName = null)
        where TSecret : SecretBase, new()
    {
        var secret = await GetSecretAsync<TSecret>(name);
        if (secret is not null)
        {
            return secret;
        }

        var secretInfo = new SecretInfo
        {
            Name = name,
            Store = nameof(DatabaseSecretStore),
            Type = typeof(TSecret).Name,
        };

        secret = CreateSecret(typeof(TSecret).Name) as TSecret;

        secret.Name = name;

        configure?.Invoke(secret);

        if (sourceName is not null && sourceName != name && (await GetSecretAsync<TSecret>(sourceName)) is not null)
        {
            await UpdateSecretAsync(secretInfo, secret, sourceName);
        }
        else
        {
            await UpdateSecretAsync(secretInfo, secret);
        }

        return secret;
    }

    private async Task<SecretBase> GetSecretAsync(SecretInfo secretInfo)
    {
        if (!_activators.TryGetValue(secretInfo.Type, out var factory) ||
            !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            return null;
        }

        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(secretInfo.Store));
        if (secretStore is null)
        {
            return null;
        }

        var secret = (await secretStore.GetSecretAsync(secretInfo.Name, factory.Type)) ?? factory.Create();

        secret.Name = secretInfo.Name;

        return secret;
    }

    public async Task<IDictionary<string, SecretInfo>> GetSecretInfosAsync()
    {
        var document = await _secretInfosManager.GetSecretInfosAsync();
        return document.SecretInfos;
    }

    public async Task<IDictionary<string, SecretInfo>> LoadSecretInfosAsync()
    {
        var document = await _secretInfosManager.LoadSecretInfosAsync();
        return document.SecretInfos;
    }

    public IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos() => _storeInfos;

    public async Task UpdateSecretAsync(SecretBase secret)
    {
        var secretInfos = await GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(secret.Name, out var secretInfo))
        {
            throw new InvalidOperationException($"The secret '{secret.Name}' doesn't exist.");
        }

        await UpdateSecretAsync(secretInfo, secret);
    }

    public async Task UpdateSecretAsync(SecretInfo secretInfo, SecretBase secret, string sourceName = null)
    {
        if (!string.Equals(secretInfo.Name, secretInfo.Name.ToSafeSecretName(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The name contains invalid characters.");
        }

        secret.Name = secretInfo.Name;

        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(secretInfo.Store));
        if (secretStore is not null)
        {
            await RemoveSecretAsync(sourceName ?? secretInfo.Name);

            await _secretInfosManager.UpdateSecretInfoAsync(secretInfo.Name, secretInfo);

            if (!secretStore.IsReadOnly)
            {
                await secretStore.UpdateSecretAsync(secretInfo.Name, secret);
            }
        }
        else
        {
            throw new InvalidOperationException($"The specified store '{secretInfo.Store}' was not found.");
        }
    }

    public async Task RemoveSecretAsync(string name)
    {
        var secretInfos = await GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(name, out var secretInfo))
        {
            return;
        }

        await RemoveSecretAsync(secretInfo);
    }

    public async Task<bool> TryRemoveSecretAsync(string name)
    {
        var secretInfos = await GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(name, out var secretInfo))
        {
            return false;
        }

        await RemoveSecretAsync(secretInfo);

        return true;
    }

    private async Task RemoveSecretAsync(SecretInfo secretInfo)
    {
        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(secretInfo.Store))
            ?? throw new InvalidOperationException($"The specified store '{secretInfo.Store}' was not found.");

        await _secretInfosManager.RemoveSecretInfoAsync(secretInfo.Name);

        // Updating a readonly store is a noop.
        if (secretStore.IsReadOnly)
        {
            return;
        }

        await secretStore.RemoveSecretAsync(secretInfo.Name);
    }
}
