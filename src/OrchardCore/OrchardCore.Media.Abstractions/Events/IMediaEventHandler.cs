namespace OrchardCore.Media.Events
{
    public interface IMediaEventHandler
    {
        void MediaCreating(MediaCreationContext context);
        void MediaCreated(MediaCreationContext context);
        void MediaDeleting(MediaContext context);
        void MediaDeleted(MediaContext context);
        void MediaDeletedUncomplete(MediaContext context);
    }
}
