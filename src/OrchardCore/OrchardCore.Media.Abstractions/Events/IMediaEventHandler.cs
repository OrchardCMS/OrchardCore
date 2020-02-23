using System.Threading.Tasks;

namespace OrchardCore.Media.Events
{
    /// <summary>
    /// Event handler fired during operations for existing media.
    /// </summary>
    public interface IMediaEventHandler
    {
        Task MediaDeletingFileAsync(MediaDeletingContext context);
        Task MediaDeletedFileAsync(MediaDeletedContext context);
        Task MediaDeletingDirectoryAsync(MediaDeletingContext context);
        Task MediaDeletedDirectoryAsync(MediaDeletedContext context);
        Task MediaMovingAsync(MediaMoveContext context);
        Task MediaMovedAsync(MediaMoveContext context);
    }
}
