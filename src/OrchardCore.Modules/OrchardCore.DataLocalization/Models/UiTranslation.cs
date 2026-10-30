namespace OrchardCore.DataLocalization.Models;

/// <summary>
/// A plain-text UI translation identified by its exact gettext context and identifier.
/// </summary>
public sealed class UiTranslation
{
    /// <summary>Gets or sets the gettext context.</summary>
    public string Context { get; set; }

    /// <summary>Gets or sets the singular source identifier.</summary>
    public string Key { get; set; }

    /// <summary>Gets or sets the plural source identifier, or null for singular entries.</summary>
    public string Plural { get; set; }

    /// <summary>Gets or sets the translations in the culture's plural-rule order.</summary>
    public string[] Values { get; set; } = [];
}
