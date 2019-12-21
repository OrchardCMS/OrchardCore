namespace OrchardCore.Media.Events
{
    public interface IMediaEventHandler
    {
        void MediaCreating(MediaCreationContext context);
        void MediaCreated(MediaCreationContext context);
        void MediaDeleting(MediaRemoveContext context);
        void MediaDeleted(MediaRemoveContext context);
        void MediaDeletedUncomplete(MediaRemoveContext context);
    }
}
