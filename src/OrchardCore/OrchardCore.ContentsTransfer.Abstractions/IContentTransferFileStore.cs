using OrchardCore.FileStorage;

namespace OrchardCore.ContentsTransfer;

/// <summary>
/// A dedicated file store for content transfer import and export files.
/// Extends <see cref="IFileStore"/> to provide isolated storage for uploaded Excel files
/// (imports) and generated export files. Files are typically stored under the tenant's
/// App_Data directory in a "ContentTransfer" subfolder.
/// </summary>
public interface IContentTransferFileStore : IFileStore
{
}
