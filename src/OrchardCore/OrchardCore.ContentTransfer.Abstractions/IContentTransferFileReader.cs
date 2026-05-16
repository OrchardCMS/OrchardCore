namespace OrchardCore.ContentTransfer;

/// <summary>
/// Reads content transfer data from a file stream.
/// </summary>
public interface IContentTransferFileReader : IDisposable
{
    /// <summary>
    /// Gets the column names from the header row.
    /// </summary>
    IReadOnlyList<string> GetColumnNames();

    /// <summary>
    /// Gets the total number of data rows (excluding the header row).
    /// </summary>
    int GetRowCount();

    /// <summary>
    /// Reads data rows sequentially. Each row is an array of string values
    /// indexed by column position. Null or empty strings indicate empty cells.
    /// </summary>
    IEnumerable<string[]> ReadRows();
}
