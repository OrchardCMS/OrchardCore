using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// A file format tabular data can be written to and read from, such as CSV, JSON or Excel. Register an implementation
/// as a singleton with <c>services.AddDataFileFormat&lt;TFormat&gt;()</c> to offer it to every consumer.
/// </summary>
public interface IDataFileFormat
{
    /// <summary>
    /// Gets the stable technical name of the format. Saved definitions store this name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the label shown to users.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the file extension of the format, with its leading dot, such as <c>.csv</c>.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Gets the media type of the format, such as <c>text/csv</c>.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Writes rows to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to. Formats that need to seek, such as Excel, require a seekable stream.</param>
    /// <param name="fields">The fields of the rows, which become the columns of the file.</param>
    /// <param name="batches">The batches of rows to write. Every batch must have the same fields as <paramref name="fields"/>.</param>
    /// <param name="options">The format options.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The number of rows written.</returns>
    Task<long> WriteAsync(
        Stream stream,
        IReadOnlyList<DataField> fields,
        IAsyncEnumerable<DataBatch> batches,
        DataFileOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the rows of a stream, one batch at a time.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="options">The format options.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The batches of rows read. A file with no rows yields one empty batch that carries its fields.</returns>
    IAsyncEnumerable<DataBatch> ReadAsync(
        Stream stream,
        DataFileOptions options,
        CancellationToken cancellationToken = default);
}
