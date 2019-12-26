using System.Threading.Tasks;

namespace OrchardCore.Media.Events
{
    /// <summary>
    /// Event handler fired during operations for existing media.
    /// </summary>
    public interface IMediaEventHandler
    {
        Task MediaDeletingFileAsync(MediaDeleteContext context);
        Task MediaDeletedFileSuccessAsync(MediaDeleteContext context);
        Task MediaDeletingFileFailureAsync(MediaDeleteContext context);
        Task MediaDeletingDirectoryAsync(MediaDeleteContext context);
        Task MediaDeletedDirectorySuccessAsync(MediaDeleteContext context);
        Task MediaDeletingDirectoryFailureAsync(MediaDeleteContext context);
        Task MediaMovingAsync(MediaMovingContext context);
    }
}
