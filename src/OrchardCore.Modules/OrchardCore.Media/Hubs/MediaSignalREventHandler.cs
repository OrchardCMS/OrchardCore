using Microsoft.AspNetCore.SignalR;
using OrchardCore.Media.Events;

namespace OrchardCore.Media.Hubs;

public class MediaSignalREventHandler : IMediaEventHandler
{
    private readonly IHubContext<MediaHub> _hubContext;

    public MediaSignalREventHandler(IHubContext<MediaHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MediaDeletedFileAsync(MediaDeletedContext context)
        => _hubContext.Clients.Group(FolderGroup(context.Path)).SendAsync("MediaChanged", new
        {
            action = "fileDeleted",
            path = context.Path,
        });

    public Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
        => _hubContext.Clients.Group(FolderGroup(context.Path)).SendAsync("MediaChanged", new
        {
            action = "directoryDeleted",
            path = context.Path,
        });

    public Task MediaMovedAsync(MediaMoveContext context)
    {
        var oldGroup = FolderGroup(context.OldPath);
        var newGroup = FolderGroup(context.NewPath);
        var payload = new { action = "fileMoved", path = context.OldPath, newPath = context.NewPath };

        // If both paths share the same parent directory, avoid sending a duplicate notification.
        if (oldGroup == newGroup)
        {
            return _hubContext.Clients.Group(oldGroup).SendAsync("MediaChanged", payload);
        }

        return _hubContext.Clients.Groups(oldGroup, newGroup).SendAsync("MediaChanged", payload);
    }

    public Task MediaCreatedDirectoryAsync(MediaCreatedContext context)
        => _hubContext.Clients.Group(FolderGroup(context.Path)).SendAsync("MediaChanged", new
        {
            action = "directoryCreated",
            path = context.Path,
        });

    public Task MediaCreatedFileAsync(MediaCreatedContext context)
        => _hubContext.Clients.Group(FolderGroup(context.Path)).SendAsync("MediaChanged", new
        {
            action = "fileUploaded",
            path = context.Path,
        });

    public Task MediaCopiedFileAsync(MediaMoveContext context)
    {
        var oldGroup = FolderGroup(context.OldPath);
        var newGroup = FolderGroup(context.NewPath);
        var payload = new { action = "fileCopied", path = context.OldPath, newPath = context.NewPath };

        if (oldGroup == newGroup)
        {
            return _hubContext.Clients.Group(oldGroup).SendAsync("MediaChanged", payload);
        }

        return _hubContext.Clients.Groups(oldGroup, newGroup).SendAsync("MediaChanged", payload);
    }

    // Returns the SignalR group name for the parent directory of the supplied path.
    // Clients subscribe to the directory they are viewing, so file/directory events must be
    // routed to the parent's group (e.g. a file at "/folder/img.jpg" notifies "/folder" viewers).
    private static string FolderGroup(string path)
        => MediaHub.GetFolderGroupName(GetParentFolderPath(path));

    private static string GetParentFolderPath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimEnd('/');
        var lastSlash = normalized.LastIndexOf('/');

        // Path is already at the root level (e.g. "/file.jpg" or "file.jpg").
        return lastSlash <= 0 ? string.Empty : normalized[..lastSlash];
    }
}
