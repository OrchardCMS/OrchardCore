namespace OrchardCore.ContentTransfer;

/// <summary>
/// Provides file format support for content transfer operations (import/export).
/// </summary>
public interface IContentTransferFileFormatProvider
{
    /// <summary>
    /// Gets the file extension including the dot (e.g., ".xlsx", ".csv").
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the MIME content type for this format.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Determines whether this provider can handle files with the given extension.
    /// </summary>
    bool CanHandle(string fileName);

    /// <summary>
    /// Creates a reader for importing data from the given stream.
    /// </summary>
    IContentTransferFileReader CreateReader(Stream stream);

    /// <summary>
    /// Creates a writer for exporting data to the given stream.
    /// </summary>
    /// <param name="stream">The output stream to write to.</param>
    /// <param name="sheetName">A display name for the data set (used as sheet name in Excel, ignored in CSV).</param>
    IContentTransferFileWriter CreateWriter(Stream stream, string sheetName);
}
