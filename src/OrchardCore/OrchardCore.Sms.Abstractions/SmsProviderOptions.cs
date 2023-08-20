using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrchardCore.Sms;

public class SmsProviderOptions
{
    private Dictionary<string, Type> _providers { get; } = new();

    public ReadOnlyDictionary<string, Type> Providers => _providers.AsReadOnly();

    /// <summary>
    /// Adds a provider if one does not exist.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <param name="type">The type of the provider.</param>
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions TryAddProvider(string name, Type type)
    {
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.");
        }

        if (_providers.ContainsKey(name))
        {
            return this;
        }

        if (!typeof(ISmsProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(ISmsProvider)}' interface.");
        }

        _providers.Add(name, type);

        return this;
    }

    /// <summary>
    /// Removes a provider if one exist.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions RemoveProvider(string name)
    {
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.");
        }

        _providers.Remove(name);

        return this;
    }

    /// <summary>
    /// Replaces existing or adds a new provider.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <param name="type">The type of the provider.</param>
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions ReplaceProvider(string name, Type type)
    {
        _providers.Remove(name);

        return TryAddProvider(name, type);
    }
}
