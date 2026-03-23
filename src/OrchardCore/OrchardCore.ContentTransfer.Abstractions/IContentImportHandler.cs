namespace OrchardCore.ContentTransfer;

/// <summary>
/// Handles import and export operations at the content item level.
/// Implementations process content item properties that are not specific to any part or field,
/// such as ContentItemId, CreatedUtc, and ModifiedUtc.
/// </summary>
public interface IContentImportHandler
{
    /// <summary>
    /// Returns the column definitions that this handler supports for import and export.
    /// Each column describes a data field that can be mapped to/from a spreadsheet column.
    /// </summary>
    /// <param name="context">The context containing the content type definition and content item.</param>
    /// <returns>A read-only collection of import column definitions.</returns>
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context);

    /// <summary>
    /// Imports data from a spreadsheet row into the content item.
    /// Maps values from <see cref="ContentImportContext.Row"/> to <see cref="ContentImportContext.ContentItem"/>.
    /// </summary>
    /// <param name="content">The import context containing the data row and target content item.</param>
    Task ImportAsync(ContentImportContext content);

    /// <summary>
    /// Exports data from the content item into a spreadsheet row.
    /// Maps values from <see cref="ContentExportContext.ContentItem"/> to <see cref="ContentExportContext.Row"/>.
    /// </summary>
    /// <param name="content">The export context containing the content item and target data row.</param>
    Task ExportAsync(ContentExportContext content);

    // Task ValidateAsync(ValidateImportContext context);
}
