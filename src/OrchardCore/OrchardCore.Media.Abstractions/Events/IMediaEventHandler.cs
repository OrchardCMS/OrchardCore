namespace OrchardCore.Media.Events
{
    public interface IMediaEventHandler
    {
        void MediaCreating(MediaCreatingContext context);
        void MediaCreated(MediaCreatedContext context);
        void MediaRemoving(MediaRemovingContext context);
        void MediaRemoved(MediaRemovedContext context);
        void MediaImporting(MediaImportingContext context);
        void MediaImported(MediaImportedContext context);
        
    }
}
