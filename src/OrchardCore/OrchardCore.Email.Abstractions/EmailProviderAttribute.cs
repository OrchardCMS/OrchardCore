namespace OrchardCore.Email;

/// <summary>
/// Describes an <see cref="IEmailProvider"/> implementation. Every concrete email provider must declare it.
/// </summary>
/// <remarks>
/// The values describe the provider type, not its instances, so they can be read without creating the provider.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EmailProviderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailProviderAttribute"/> class.
    /// </summary>
    /// <param name="technicalName">The technical name of the provider.</param>
    /// <param name="displayName">The untranslated display name of the provider, localized in the context of the provider type.</param>
    public EmailProviderAttribute(string technicalName, string displayName)
    {
        ArgumentException.ThrowIfNullOrEmpty(technicalName);
        ArgumentException.ThrowIfNullOrEmpty(displayName);

        TechnicalName = technicalName;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the technical name of the provider, used to select it in the settings and in <see cref="IEmailProviderResolver"/>.
    /// </summary>
    public string TechnicalName { get; }

    /// <summary>
    /// Gets the untranslated display name of the provider, localized in the context of the provider type.
    /// </summary>
    public string DisplayName { get; }
}
