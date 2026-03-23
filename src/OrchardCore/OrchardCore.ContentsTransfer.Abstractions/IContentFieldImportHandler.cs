namespace OrchardCore.ContentsTransfer;

/// <summary>
/// Handles import and export operations for a specific content field type.
/// Each implementation is resolved by field name via <see cref="IContentImportHandlerResolver.GetFieldHandlers"/>.
/// The <c>StandardFieldImportHandler</c> base class in the Core project provides a convenient implementation for most field types.
/// </summary>
public interface IContentFieldImportHandler
{
    /// <summary>
    /// Returns the column definitions that this field handler supports for import and export.
    /// Column names follow the convention: <c>{PartName}_{FieldName}_{PropertyName}</c>.
    /// </summary>
    /// <param name="context">The context containing the content part field definition and part name.</param>
    /// <returns>A read-only collection of import column definitions for this field.</returns>
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context);

    /// <summary>
    /// Imports a value from the spreadsheet row and sets it on the content field.
    /// </summary>
    /// <param name="context">The import context containing the data row, content part, and field definition.</param>
    Task ImportAsync(ContentFieldImportMapContext context);

    /// <summary>
    /// Exports a value from the content field into the spreadsheet row.
    /// </summary>
    /// <param name="context">The export context containing the content field, part, and target data row.</param>
    Task ExportAsync(ContentFieldExportMapContext context);
}
