using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Contract for providing .NET cultures, including culture aliases.
/// </summary>
public interface ICultureProvider
{
    /// <summary>
    /// Gets all cultures recognized by .NET, including culture aliases.
    /// </summary>
    CultureInfo[] GetAllCulturesAndAliases();
}
