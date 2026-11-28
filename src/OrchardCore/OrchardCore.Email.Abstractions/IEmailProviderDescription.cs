using OrchardCore.Localization;

namespace OrchardCore.Email;

/// <summary>
/// Describes an <see cref="IEmailProvider"/> with the static members of its class, so the providers can be listed
/// without creating them. Every provider registered with <c>services.AddEmailProvider&lt;T&gt;(name)</c> implements it.
/// It is separate from <see cref="IEmailProvider"/>, because an interface with static abstract members can't be used
/// as a type argument.
/// </summary>
public interface IEmailProviderDescription
{
    /// <summary>
    /// Gets the display name of the provider, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    static abstract LocalizationSource DisplayName { get; }
}
