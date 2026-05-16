namespace OrchardCore.ContentTransfer;

/// <summary>
/// Writes content transfer data to a file stream.
/// </summary>
public interface IContentTransferFileWriter : IDisposable
{
    /// <summary>
    /// Writes the header row with column names. Must be called before writing any data rows.
    /// </summary>
    void WriteHeader(IReadOnlyList<string> columnNames);

    /// <summary>
    /// Writes a single data row. Values are indexed by column position matching the header.
    /// </summary>
    void WriteRow(IReadOnlyList<string> values);

    /// <summary>
    /// Flushes any buffered data and finalizes the file. Must be called when writing is complete.
    /// </summary>
    void Flush();
}
