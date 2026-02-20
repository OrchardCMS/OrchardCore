namespace OrchardCore.Media.Events;

/// <summary>
/// Event handler fired during operations for existing media.
/// </summary>
public interface IMediaEventHandler
{
    Task MediaDeletingFileAsync(MediaDeletingContext context) => Task.CompletedTask;
    Task MediaDeletedFileAsync(MediaDeletedContext context) => Task.CompletedTask;
    Task MediaDeletingDirectoryAsync(MediaDeletingContext context) => Task.CompletedTask;
    Task MediaDeletedDirectoryAsync(MediaDeletedContext context) => Task.CompletedTask;
    Task MediaMovingAsync(MediaMoveContext context) => Task.CompletedTask;
    Task MediaMovedAsync(MediaMoveContext context) => Task.CompletedTask;
    Task MediaCreatingDirectoryAsync(MediaCreatingContext context) => Task.CompletedTask;
    Task MediaCreatedDirectoryAsync(MediaCreatedContext context) => Task.CompletedTask;
    Task MediaPermittedStorageAsync(MediaPermittedStorageContext context) => Task.CompletedTask;
}
