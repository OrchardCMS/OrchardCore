namespace OrchardCore.DataSources.Files;

/// <summary>
/// Gives access to the registered data file formats.
/// </summary>
public interface IDataFileFormatManager
{
    /// <summary>
    /// Lists the registered formats, in the order they were registered, without creating them.
    /// </summary>
    /// <returns>The descriptors of the formats.</returns>
    IReadOnlyList<DataFileFormatDescriptor> GetFormats();

    /// <summary>
    /// Finds the descriptor of a format by its technical name, ignoring case.
    /// </summary>
    /// <param name="name">The technical name of the format.</param>
    /// <returns>The descriptor, or <see langword="null"/> when none is registered with that name.</returns>
    DataFileFormatDescriptor GetDescriptor(string name);

    /// <summary>
    /// Finds the descriptor of the format of a file from its extension, ignoring case.
    /// </summary>
    /// <param name="fileName">The file name or path.</param>
    /// <returns>The descriptor, or <see langword="null"/> when no registered format uses that extension.</returns>
    DataFileFormatDescriptor GetDescriptorByFileName(string fileName);

    /// <summary>
    /// Gets a format by its technical name, ignoring case.
    /// </summary>
    /// <param name="name">The technical name of the format.</param>
    /// <returns>The format, or <see langword="null"/> when none is registered with that name.</returns>
    IDataFileFormat GetFormat(string name);
}
