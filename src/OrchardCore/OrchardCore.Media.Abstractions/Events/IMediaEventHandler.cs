using System.Threading.Tasks;

namespace OrchardCore.Media.Events
{
    public interface IMediaEventHandler
    {
        Task MediaCreatingAsync(MediaCreatingContext context);        
        Task MediaDeletingAsync(MediaDeleteContext context);
        Task MediaDeletedSuccessfullyAsync(MediaDeleteContext context);
        Task MediaDeletedUnsuccessfullyAsync(MediaDeleteContext context);
        Task MediaMovingAsync(MediaMovingContext context);
    }
}
