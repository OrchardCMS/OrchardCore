namespace OrchardCore.Localization;

/// <summary>
/// Represents a programmable options for localization culture.
/// </summary>
public class CultureOptions
{
    /// <summary>
    /// Gets or sets whether to ignore the system culture settings or not.
    /// </summary>
    /// <remarks>
    /// The current culture should not depend on local computer settings by default.
    /// For more information refer to https://github.com/OrchardCMS/OrchardCore/issues/11228
    /// </remarks>
    public bool IgnoreSystemSettings { get; set; }
}
