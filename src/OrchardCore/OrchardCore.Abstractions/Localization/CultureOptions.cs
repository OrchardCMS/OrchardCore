namespace OrchardCore.Localization;

/// <summary>
/// Represents a programmable options for localization culture.
/// </summary>
public class CultureOptions
{
    /// <summary>
    /// Gets or sets whether to use default culture settings or not. Defaults to <c>false</c>.
    /// </summary>
    public bool IgnoreSystemSettings { get; set; }
}
