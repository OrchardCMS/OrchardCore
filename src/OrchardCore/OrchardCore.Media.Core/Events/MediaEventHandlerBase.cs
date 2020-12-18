using System.Threading.Tasks;
using OrchardCore.Media.Events;

namespace OrchardCore.Media.Core.Events
{
    public class MediaEventHandlerBase : IMediaEventHandler
    {
        public virtual Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task MediaDeletedFileAsync(MediaDeletedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task MediaDeletingDirectoryAsync(MediaDeletingContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task MediaDeletingFileAsync(MediaDeletingContext context)
        {
            return Task.CompletedTask;
        }

        public Task MediaMovingAsync(MediaMoveContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task MediaMovedAsync(MediaMoveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
