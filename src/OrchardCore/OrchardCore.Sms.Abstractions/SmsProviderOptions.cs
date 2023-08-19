using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrchardCore.Sms;

public class SmsProviderOptions
{
    public ReadOnlyDictionary<string, Type> Providers => _providers.AsReadOnly();

    private Dictionary<string, Type> _providers { get; } = new();

    /// <summary>
    /// Provides a way to add a provider if one does not already exists.
    /// </summary>
    /// <param name="name">The name of the provider.</param>
    /// <param name="type">The type of the provider.</param>
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

    public SmsProviderOptions RemoveProvider(string name)
    {
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.");
        }

        _providers.Remove(name);

        return this;
    }

    public SmsProviderOptions ReplaceProvider(string name, Type type)
    {
        _providers.Remove(name);

        return TryAddProvider(name, type);
    }
}
