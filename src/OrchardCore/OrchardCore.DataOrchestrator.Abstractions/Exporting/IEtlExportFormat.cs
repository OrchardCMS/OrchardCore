using System.Text.Json.Nodes;

namespace OrchardCore.DataOrchestrator.Exporting;

/// <summary>
/// Represents an extensible serialization format used by file-based ETL destinations.
/// Implementations turn the records flowing through a pipeline into a byte stream
/// (for example JSON, CSV, or an Excel workbook) that a destination can persist.
/// </summary>
public interface IEtlExportFormat
{
    /// <summary>
    /// Gets the technical name of the format (for example <c>Json</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the display text of the format.
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// Gets the default file extension produced by this format, without a leading dot (for example <c>json</c>).
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the MIME content type produced by this format.
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Serializes the specified records to the provided output stream.
    /// </summary>
    /// <param name="records">The records to serialize.</param>
    /// <param name="output">The stream the serialized content is written to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task WriteAsync(IAsyncEnumerable<JsonObject> records, Stream output, CancellationToken cancellationToken);
}
