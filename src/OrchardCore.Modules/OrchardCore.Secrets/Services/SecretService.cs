using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;

namespace OrchardCore.Secrets.Services;

public class SecretService : ISecretService
{
    private readonly SecretBindingsManager _bindingsManager;
    private readonly IReadOnlyCollection<SecretStoreInfo> _storeInfos;
    private readonly IEnumerable<ISecretStore> _stores;

    private readonly Dictionary<string, SecretActivator> _activators = new();

    public SecretService(SecretBindingsManager bindingsManager, IEnumerable<ISecretStore> stores, IOptions<SecretOptions> options)
    {
        _bindingsManager = bindingsManager;

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

    public async Task<SecretBase> GetSecretAsync(SecretBinding binding)
    {
        if (!_activators.TryGetValue(binding.Type, out var factory) ||
            !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            return null;
        }

        var secretStore = _stores.FirstOrDefault(store => String.Equals(store.Name, binding.Store, StringComparison.OrdinalIgnoreCase));
        if (secretStore is null)
        {
            return null;
        }

        var secret = (await secretStore.GetSecretAsync(binding.Name, factory.Type)) ?? factory.Create();

        secret.Name = binding.Name;

        return secret;
    }

    public async Task<SecretBase> GetSecretAsync(string name, Type type)
    {
        if (!_activators.TryGetValue(type.Name, out var factory) || !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            throw new ArgumentException($"The type should be configured and should implement '{nameof(SecretBase)}'.", nameof(type));
        }

        var bindings = await GetSecretBindingsAsync();
        if (!bindings.TryGetValue(name, out var binding))
        {
            return null;
        }

        return await GetSecretAsync(binding);
    }

    public async Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync()
    {
        var secretsDocument = await _bindingsManager.GetSecretBindingsDocumentAsync();
        return secretsDocument.SecretBindings;
    }

    public async Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync()
    {
        var secretsDocument = await _bindingsManager.LoadSecretBindingsDocumentAsync();
        return secretsDocument.SecretBindings;
    }

    public IReadOnlyCollection<SecretStoreInfo> GetSecretStoreInfos() => _storeInfos;

    public async Task UpdateSecretAsync(string name, SecretBinding binding, SecretBase secret)
    {
        if (!String.Equals(name, name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The name contains invalid characters.");
        }

        var secretStore = _stores.FirstOrDefault(store => String.Equals(store.Name, binding.Store, StringComparison.OrdinalIgnoreCase));
        if (secretStore is not null)
        {
            await _bindingsManager.UpdateSecretBindingAsync(name, binding);

            // This is a noop rather than an exception as updating a readonly store is considered a noop.
            if (!secretStore.IsReadOnly)
            {
                secret.Name = binding.Name;

                await secretStore.UpdateSecretAsync(name, secret);
            }
        }
        else
        {
            throw new InvalidOperationException($"The specified store '{binding.Store}' was not found.");
        }
    }

    public async Task RemoveSecretAsync(string name, string storeName)
    {
        var store = _stores.FirstOrDefault(store => String.Equals(store.Name, storeName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"The specified store '{storeName}' was not found.");

        await _bindingsManager.RemoveSecretBindingAsync(name);

        // This is a noop rather than an exception as updating a readonly store is considered a noop.
        if (!store.IsReadOnly)
        {
            await store.RemoveSecretAsync(name);
        }
    }
}
