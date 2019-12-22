using System.Threading.Tasks;

namespace OrchardCore.Media.Events
{
    public interface IMediaEventHandler
    {
        Task MediaCreatingAsync(MediaCreatingContext context);        
        Task MediaDeletingAsync(MediaDeleteContext context);
        Task MediaDeletedSuccessAsync(MediaDeleteContext context);
        Task MediaDeletedUnsuccessAsync(MediaDeleteContext context);
        Task MediaMovingAsync(MediaMovingContext context);
    }
}
