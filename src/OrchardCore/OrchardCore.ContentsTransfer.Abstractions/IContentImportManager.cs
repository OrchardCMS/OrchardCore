namespace OrchardCore.ContentsTransfer;

/// <summary>
/// The main service that orchestrates the import and export of content items.
/// Coordinates <see cref="IContentImportHandler"/>, <see cref="IContentPartImportHandler"/>,
/// and <see cref="IContentFieldImportHandler"/> to map data between spreadsheet rows and content items.
/// </summary>
public interface IContentImportManager
{
    /// <summary>
    /// Collects all available column definitions for a content type by invoking all registered
    /// content, part, and field handlers. Used to generate spreadsheet headers and UI schemas.
    /// </summary>
    /// <param name="context">The context containing the content type definition.</param>
    /// <returns>A read-only collection of all import columns supported by the content type.</returns>
    Task<IReadOnlyCollection<ImportColumn>> GetColumnsAsync(ImportContentContext context);

    /// <summary>
    /// Maps a spreadsheet data row to a content item by invoking all registered content, part,
    /// and field import handlers. The handlers read values from the data row and set them
    /// on the content item's parts and fields.
    /// </summary>
    /// <param name="context">The import context containing the data row and target content item.</param>
    Task ImportAsync(ContentImportContext context);

    /// <summary>
    /// Maps a content item to a spreadsheet data row by invoking all registered content, part,
    /// and field export handlers. The handlers read values from the content item's parts and fields
    /// and write them to the data row.
    /// </summary>
    /// <param name="context">The export context containing the content item and target data row.</param>
    Task ExportAsync(ContentExportContext context);
}
