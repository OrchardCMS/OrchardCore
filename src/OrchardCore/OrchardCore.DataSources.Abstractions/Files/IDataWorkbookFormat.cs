namespace OrchardCore.DataSources.Files;

/// <summary>
/// A file format whose files hold several data sets, each in its own worksheet, such as an Excel workbook.
/// </summary>
public interface IDataWorkbookFormat : IDataFileFormat
{
    /// <summary>
    /// Writes several data sets to a stream, each in its own worksheet of one workbook.
    /// </summary>
    /// <remarks>
    /// The worksheets are written in order, and the rows of each one are written as they arrive: the
    /// <see cref="DataSheet.Batches"/> of a sheet are enumerated completely before those of the next sheet are started.
    /// Callers must make sure that the batches of a later sheet can wait until the previous sheets are written, for
    /// example by not producing them from a shared reader that is still in use.
    /// </remarks>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="sheets">The data sets to write, one worksheet each, in the order of the worksheets.</param>
    /// <param name="options">The format options. <see cref="DataFileOptions.SheetName"/> is ignored, each sheet has its own name.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The number of rows written to each sheet, in the order of <paramref name="sheets"/>.</returns>
    Task<IReadOnlyList<long>> WriteSheetsAsync(
        Stream stream,
        IReadOnlyList<DataSheet> sheets,
        DataFileOptions options,
        CancellationToken cancellationToken = default);
}
