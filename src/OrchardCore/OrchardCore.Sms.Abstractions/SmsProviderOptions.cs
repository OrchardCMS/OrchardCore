using System.Collections.Frozen;

namespace OrchardCore.Sms;

public class SmsProviderOptions
{
    private Dictionary<string, SmsProviderTypeOptions> _providers { get; } = [];

    private FrozenDictionary<string, SmsProviderTypeOptions> _readonlyProviders;

    /// <summary>
    /// This read-only collections contains all registered SMS providers.
    /// The 'Key' is the technical name of the provider.
    /// The 'Value' is the type of the SMS provider. The type will always be an implementation of <see cref="ISmsProvider"></see> interface.
    /// </summary>
    public IReadOnlyDictionary<string, SmsProviderTypeOptions> Providers
        => _readonlyProviders ??= _providers.ToFrozenDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Adds a provider if one does not exist.
    /// </summary>
    /// <param name="name">The technical name of the provider.</param>
    /// <param name="options">The type options of the provider.</param>
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions TryAddProvider(string name, SmsProviderTypeOptions options)
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
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions RemoveProvider(string name)
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
    /// <returns cref="SmsProviderOptions"></returns>
    /// <exception cref="ArgumentException"></exception>
    public SmsProviderOptions ReplaceProvider(string name, SmsProviderTypeOptions options)
    {
        _providers.Remove(name);

        return TryAddProvider(name, options);
    }
}

public class SmsProviderTypeOptions
{
    public Type Type { get; }

    public SmsProviderTypeOptions(Type type)
    {
        if (!typeof(ISmsProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(ISmsProvider)}' interface.");
        }

        Type = type;
    }

    public bool IsEnabled { get; set; }
}
