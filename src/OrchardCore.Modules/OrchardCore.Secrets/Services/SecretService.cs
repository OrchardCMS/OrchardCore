using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;
using OrchardCore.Secrets.Stores;

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

    public async Task<SecretBase> GetSecretAsync(string name)
    {
        var bindings = await GetSecretBindingsAsync();
        if (!bindings.TryGetValue(name, out var binding))
        {
            return null;
        }

        return await GetSecretAsync(binding);
    }

    public TSecret CreateSecret<TSecret>() where TSecret : SecretBase, new() => CreateSecret(typeof(TSecret).Name) as TSecret;

    public async Task<TSecret> GetSecretAsync<TSecret>(string name) where TSecret : SecretBase, new()
    {
        var bindings = await GetSecretBindingsAsync();
        if (!bindings.TryGetValue(name, out var binding))
        {
            return null;
        }

        return await GetSecretAsync(binding) as TSecret;
    }

    public async Task<TSecret> GetOrCreateSecretAsync<TSecret>(string name, Action<TSecret> configure = null, string sourceName = null)
        where TSecret : SecretBase, new()
    {
        var secret = await GetSecretAsync<TSecret>(name);
        if (secret is not null)
        {
            return secret;
        }

        var binding = new SecretBinding
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
            await UpdateSecretAsync(binding, secret, sourceName);
        }
        else
        {
            await UpdateSecretAsync(binding, secret);
        }

        return secret;
    }

    public async Task<SecretBase> GetSecretAsync(SecretBinding binding)
    {
        if (!_activators.TryGetValue(binding.Type, out var factory) ||
            !typeof(SecretBase).IsAssignableFrom(factory.Type))
        {
            return null;
        }

        var secretStore = _stores.FirstOrDefault(store => string.Equals(store.Name, binding.Store, StringComparison.OrdinalIgnoreCase));
        if (secretStore is null)
        {
            return null;
        }

        var secret = (await secretStore.GetSecretAsync(binding.Name, factory.Type)) ?? factory.Create();

        secret.Name = binding.Name;

        return secret;
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

    public async Task UpdateSecretAsync(SecretBase secret)
    {
        var secretBindings = await GetSecretBindingsAsync();
        if (!secretBindings.TryGetValue(secret.Name, out var binding))
        {
            throw new InvalidOperationException($"The secret '{secret.Name}' doesn't exist.");
        }

        await UpdateSecretAsync(binding, secret);
    }

    public async Task UpdateSecretAsync(SecretBinding binding, SecretBase secret, string sourceName = null)
    {
        SecretBinding existingBinding = null;
        if (sourceName is not null)
        {
            var secretBindings = await GetSecretBindingsAsync();
            if (!secretBindings.TryGetValue(sourceName, out existingBinding))
            {
                throw new InvalidOperationException($"The secret '{secret.Name}' doesn't exist.");
            }
        }

        if (!string.Equals(binding.Name, binding.Name.ToSafeNamespace(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The name contains invalid characters.");
        }

        secret.Name = binding.Name;

        var secretStore = _stores.FirstOrDefault(store => string.Equals(store.Name, binding.Store, StringComparison.OrdinalIgnoreCase));
        if (secretStore is not null)
        {
            // Remove existing binding first.
            if (existingBinding is not null)
            {
                await RemoveSecretAsync(existingBinding);
            }

            await _bindingsManager.UpdateSecretBindingAsync(binding.Name, binding);

            // Updating a readonly store is a noop.
            if (!secretStore.IsReadOnly)
            {
                await secretStore.UpdateSecretAsync(binding.Name, secret);
            }
        }
        else
        {
            throw new InvalidOperationException($"The specified store '{binding.Store}' was not found.");
        }
    }

    public async Task RemoveSecretAsync(SecretBinding binding)
    {
        var store = _stores.FirstOrDefault(store => string.Equals(store.Name, binding.Store, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"The specified store '{binding.Store}' was not found.");

        await _bindingsManager.RemoveSecretBindingAsync(binding.Name);

        // Updating a readonly store is a noop.
        if (store.IsReadOnly)
        {
            return;
        }

        await store.RemoveSecretAsync(binding.Name);
    }
}
