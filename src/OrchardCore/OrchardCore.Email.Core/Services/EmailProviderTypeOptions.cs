using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Email.Services;

public class EmailProviderTypeOptions
{
    public Type Type { get; }

    /// <summary>
    /// Gets the display name of the provider, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    [JsonIgnore]
    public LocalizationSource DisplayName { get; }

    public EmailProviderTypeOptions(Type type, LocalizationSource displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        if (!typeof(IEmailProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(IEmailProvider)}' interface.");
        }

        Type = type;
        DisplayName = displayName;
    }

    /// <summary>
    /// Describes an email provider from the static members of its class.
    /// </summary>
    /// <typeparam name="TProvider">The class of the email provider.</typeparam>
    /// <returns>The type options of the provider.</returns>
    public static EmailProviderTypeOptions Create<TProvider>()
        where TProvider : class, IEmailProvider, IEmailProviderDescription
    {
        return new EmailProviderTypeOptions(typeof(TProvider), TProvider.DisplayName);
    }

    public bool IsEnabled { get; set; }
}
