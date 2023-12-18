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

    public async Task UpdateSecretAsync(SecretBase secret, SecretInfo info = null, string source = null)
    {
        if (info is null)
        {
            var secretInfos = await GetSecretInfosAsync();
            if (!secretInfos.TryGetValue(secret.Name, out info))
            {
                throw new InvalidOperationException($"The secret '{secret.Name}' doesn't exist.");
            }
        }
        else if (!info.Name.EqualsOrdinalIgnoreCase(info.Name.ToSafeSecretName()))
        {
            throw new InvalidOperationException($"The secret name '{info.Name}' contains invalid characters.");
        }

        secret.Name = info.Name;
        info.Store ??= nameof(DatabaseSecretStore);
        info.Type ??= secret.GetType().Name;

        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(info.Store))
            ?? throw new InvalidOperationException($"The specified store '{info.Store}' was not found.");

        await RemoveSecretAsync(source ?? info.Name);

        await _secretInfosManager.UpdateSecretInfoAsync(info.Name, info);

        if (!secretStore.IsReadOnly)
        {
            await secretStore.UpdateSecretAsync(info.Name, secret);
        }
    }

    public async Task<bool> RemoveSecretAsync(string name)
    {
        var secretInfos = await GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(name, out var info))
        {
            return false;
        }

        await RemoveSecretAsync(info);

        return true;
    }

    private async Task<SecretBase> GetSecretAsync(SecretInfo info)
    {
        if (!_activators.TryGetValue(info.Type, out var factory) ||
            !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            return null;
        }

        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(info.Store));
        if (secretStore is null)
        {
            return null;
        }

        var secret = (await secretStore.GetSecretAsync(info.Name, factory.Type)) ?? factory.Create();

        secret.Name = info.Name;

        return secret;
    }

    private async Task RemoveSecretAsync(SecretInfo info)
    {
        var secretStore = _stores.FirstOrDefault(store => store.Name.EqualsOrdinalIgnoreCase(info.Store))
            ?? throw new InvalidOperationException($"The specified store '{info.Store}' was not found.");

        await _secretInfosManager.RemoveSecretInfoAsync(info.Name);

        if (secretStore.IsReadOnly)
        {
            return;
        }

        await secretStore.RemoveSecretAsync(info.Name);
    }
}
