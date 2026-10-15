namespace OrchardCore.DataLocalization.Models;

/// <summary>
/// An authoritative entry discovered in an application's embedded POT catalog.
/// </summary>
public sealed class UiLocalizationResource
{
    /// <summary>Gets or sets the assembly owning the catalog.</summary>
    public string AssemblyName { get; set; }

    /// <summary>Gets or sets the exact gettext context.</summary>
    public string Context { get; set; }

    /// <summary>Gets or sets the singular identifier.</summary>
    public string Key { get; set; }

    /// <summary>Gets or sets the plural identifier.</summary>
    public string Plural { get; set; }

    /// <summary>Gets the extracted comments, references and flags.</summary>
    public List<string> Metadata { get; } = [];

    /// <summary>Gets translations parsed from a PO file in index order.</summary>
    public List<string> Values { get; } = [];
}
