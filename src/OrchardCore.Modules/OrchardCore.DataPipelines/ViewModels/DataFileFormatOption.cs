using Microsoft.Extensions.Localization;
using OrchardCore.DataSources.Files;

namespace OrchardCore.DataPipelines.ViewModels;

/// <summary>
/// A file format offered by an editor, with its localized name.
/// </summary>
/// <param name="Name">The technical name of the format.</param>
/// <param name="DisplayName">The localized name of the format.</param>
/// <param name="Extension">The file extension of the format.</param>
public sealed record DataFileFormatOption(string Name, string DisplayName, string Extension)
{
    /// <summary>
    /// Lists the registered formats, ordered by their localized name.
    /// </summary>
    /// <param name="formatManager">The formats.</param>
    /// <param name="localizerFactory">Localizes the names of the formats.</param>
    /// <returns>The formats.</returns>
    public static IReadOnlyList<DataFileFormatOption> List(IDataFileFormatManager formatManager, IStringLocalizerFactory localizerFactory)
    {
        ArgumentNullException.ThrowIfNull(formatManager);
        ArgumentNullException.ThrowIfNull(localizerFactory);

        return formatManager.GetFormats()
            .Select(format => new DataFileFormatOption(format.Name, localizerFactory.Localize(format.DisplayName)?.Value ?? format.Name, format.Extension))
            .OrderBy(format => format.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }
}
