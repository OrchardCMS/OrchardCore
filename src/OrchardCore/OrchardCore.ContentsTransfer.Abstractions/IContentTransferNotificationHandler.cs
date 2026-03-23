namespace OrchardCore.ContentsTransfer;

/// <summary>
/// Handles notifications for content transfer operations.
/// Resolved optionally by the export background task to notify users when their
/// queued export is ready for download. Implemented when the Notifications module is enabled.
/// </summary>
public interface IContentTransferNotificationHandler
{
    /// <summary>
    /// Sends a notification to the user who requested the export, informing them
    /// that the export file is ready for download from the Export Dashboard.
    /// </summary>
    /// <param name="entry">The completed export entry containing the owner and file details.</param>
    /// <param name="contentTypeName">The display name of the content type that was exported.</param>
    Task NotifyExportCompletedAsync(ContentTransferEntry entry, string contentTypeName);
}
