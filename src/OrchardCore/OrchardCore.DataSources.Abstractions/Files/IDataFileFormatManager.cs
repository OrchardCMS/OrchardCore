namespace OrchardCore.DataSources.Files;

/// <summary>
/// Gives access to the registered data file formats.
/// </summary>
public interface IDataFileFormatManager
{
    /// <summary>
    /// Lists the registered formats, ordered by display name.
    /// </summary>
    /// <returns>The formats.</returns>
    IReadOnlyList<IDataFileFormat> GetFormats();

    /// <summary>
    /// Finds a format by its technical name, ignoring case.
    /// </summary>
    /// <param name="name">The technical name of the format.</param>
    /// <returns>The format, or <see langword="null"/> when none is registered with that name.</returns>
    IDataFileFormat GetFormat(string name);

    /// <summary>
    /// Finds the format of a file from its extension, ignoring case.
    /// </summary>
    /// <param name="fileName">The file name or path.</param>
    /// <returns>The format, or <see langword="null"/> when no registered format uses that extension.</returns>
    IDataFileFormat GetFormatByFileName(string fileName);
}
