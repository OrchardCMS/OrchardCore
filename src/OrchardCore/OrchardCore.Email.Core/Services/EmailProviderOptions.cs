using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Email.Core.Services;

public class EmailProviderOptions
{
    private Dictionary<string, EmailProviderTypeOptions> _providers { get; } = [];

    private FrozenDictionary<string, EmailProviderTypeOptions> _readonlyProviders;

    /// <summary>
    /// This read-only collections contains all registered SMS providers.
    /// The 'Key' is the technical name of the provider.
    /// The 'Value' is the type of the SMS provider. The type will always be an implementation of <see cref="IEmailProvider"></see> interface.
    /// </summary>
    public IReadOnlyDictionary<string, EmailProviderTypeOptions> Providers
        => _readonlyProviders ??= _providers.ToFrozenDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Adds a provider if one does not exist.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <param name="options">The type options of the provider.</param>
    /// <returns cref="EmailProviderTypeOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public EmailProviderOptions TryAddProvider(string name, EmailProviderTypeOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.");
        }

        if (_providers.ContainsKey(name))
        {
            return this;
        }

        _providers.Add(name, options);
        _readonlyProviders = null;

        return this;
    }

    /// <summary>
    /// Removes a provider if one exist.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <returns cref="EmailProviderTypeOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public EmailProviderOptions RemoveProvider(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.");
        }

        if (_providers.Remove(name))
        {
            _readonlyProviders = null;
        }

        return this;
    }

    /// <summary>
    /// Replaces existing or adds a new provider.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <param name="options">The type-options of the provider.</param>
    /// <returns cref="EmailProviderTypeOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public EmailProviderOptions ReplaceProvider(string name, EmailProviderTypeOptions options)
    {
        _providers.Remove(name);

        return TryAddProvider(name, options);
    }
}

public class EmailProviderTypeOptions
{
    public Type Type { get; }

    public EmailProviderTypeOptions(Type type)
    {
        if (!typeof(IEmailProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(IEmailProvider)}' interface.");
        }

        Type = type;
    }

    public bool IsEnabled { get; set; }
}
