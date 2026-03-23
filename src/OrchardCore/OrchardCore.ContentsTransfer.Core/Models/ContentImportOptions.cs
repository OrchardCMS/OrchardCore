namespace OrchardCore.ContentsTransfer.Models;

public sealed class ContentImportOptions
{
    public const int DefaultImportBatchSize = 100;

    public const int DefaultExportBatchSize = 200;

    public int ImportBatchSize { get; set; } = DefaultImportBatchSize;

    // 100MB is the absolute max.
    public const int AbsoluteMaxAllowedFileSizeInBytes = 100 * 1024 * 1024;

    public bool AllowAllContentTypes { get; set; } = true;

    public string[] AllowedContentTypes { get; set; } = [];

    // 20MB is the default.
    public long MaxAllowedFileSizeInBytes { get; set; } = 20 * 1024 * 1024;

    public int ExportBatchSize { get; set; } = DefaultExportBatchSize;

    /// <summary>
    /// The number of records at which exports are queued for background processing
    /// instead of being downloaded immediately.
    /// </summary>
    public int ExportQueueThreshold { get; set; } = 500;

    public long GetMaxAllowedSize()
    {
        if (MaxAllowedFileSizeInBytes < 1)
        {
            return AbsoluteMaxAllowedFileSizeInBytes;
        }

        return Math.Min(MaxAllowedFileSizeInBytes, AbsoluteMaxAllowedFileSizeInBytes);
    }

    public double GetMaxAllowedSizeInMb()
    {
        return GetMaxAllowedSize() / 1000000d;
    }
}
