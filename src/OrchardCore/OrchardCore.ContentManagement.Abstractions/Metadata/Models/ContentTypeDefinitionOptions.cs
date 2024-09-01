namespace OrchardCore.ContentManagement.Metadata.Models;

/// <summary>
/// Offers a method for configuring content type definitions to either display or conceal global settings from appearing on the UI.
/// </summary>
public class ContentTypeDefinitionOptions
{
    /// <summary>
    /// Configure the driver options for all content types that share the same Stereotype.
    /// In this dictionary, the 'key' denotes the Stereotype, while the 'value' corresponds to the driver options.
    /// </summary>
    public Dictionary<string, ContentTypeDefinitionDriverOptions> Stereotypes { get; } = [];

    /// <summary>
    /// Configure the driver options for each content type.
    /// In this dictionary, the 'key' denotes the content type, while the 'value' corresponds to the driver options.
    /// </summary>
    public Dictionary<string, ContentTypeDefinitionDriverOptions> ContentTypes { get; } = [];
}
