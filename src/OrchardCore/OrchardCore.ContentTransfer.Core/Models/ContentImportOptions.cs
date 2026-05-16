namespace OrchardCore.ContentTransfer.Models;

public sealed class ContentImportOptions
{
    public const int DefaultImportBatchSize = 100;

    public const int DefaultExportBatchSize = 200;

    public int ImportBatchSize { get; set; } = DefaultImportBatchSize;

    public bool AllowAllContentTypes { get; set; } = true;

    public string[] AllowedContentTypes { get; set; } = [];

    public int ExportBatchSize { get; set; } = DefaultExportBatchSize;

    /// <summary>
    /// The number of records at which exports are queued for background processing
    /// instead of being downloaded immediately.
    /// </summary>
    public int ExportQueueThreshold { get; set; } = 500;
}
