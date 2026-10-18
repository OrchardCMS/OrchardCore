using Annotype;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.Email.Services;

public class EmailProviderTypeOptions
{
    public Type Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailProviderTypeOptions"/> class.
    /// </summary>
    /// <param name="type">The email provider type. It must implement <see cref="IEmailProvider"/>.</param>
    /// <exception cref="MetadataNotFoundException">The <see cref="EmailProviderAttribute"/> of <paramref name="type"/> is not registered.</exception>
    public EmailProviderTypeOptions(Type type)
    {
        if (!typeof(IEmailProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(IEmailProvider)}' interface.");
        }

        // The metadata is read from the type, without creating the provider and its dependencies.
        var metadata = TypeMetadata.Get<EmailProviderAttribute>(type);

        Type = type;
        TechnicalName = metadata.TechnicalName;
        DisplayName = LocalizationSource.Create(metadata.DisplayName, type);
    }

    /// <summary>
    /// Gets the technical name of the provider, as declared by its <see cref="EmailProviderAttribute"/>.
    /// </summary>
    public string TechnicalName { get; }

    /// <summary>
    /// Gets the display name of the provider, localized in the context of the provider type.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    public bool IsEnabled { get; set; }
}
