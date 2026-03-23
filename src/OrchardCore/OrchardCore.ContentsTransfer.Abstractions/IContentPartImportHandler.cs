namespace OrchardCore.ContentsTransfer;

/// <summary>
/// Handles import and export operations for a specific content part type.
/// Each implementation is resolved by part name via <see cref="IContentImportHandlerResolver.GetPartHandlers"/>.
/// </summary>
public interface IContentPartImportHandler
{
    /// <summary>
    /// Returns the column definitions that this part handler supports for import and export.
    /// </summary>
    /// <param name="context">The context containing the content type part definition.</param>
    /// <returns>A read-only collection of import column definitions for this part.</returns>
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context);

    /// <summary>
    /// Imports data from a spreadsheet row into the content part.
    /// </summary>
    /// <param name="content">The import context containing the data row and target content item.</param>
    Task ImportAsync(ContentPartImportMapContext content);

    /// <summary>
    /// Exports data from the content part into a spreadsheet row.
    /// </summary>
    /// <param name="content">The export context containing the content part and target data row.</param>
    Task ExportAsync(ContentPartExportMapContext content);
}
