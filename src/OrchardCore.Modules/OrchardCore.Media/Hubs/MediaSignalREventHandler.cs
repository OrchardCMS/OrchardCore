using Microsoft.AspNetCore.SignalR;
using OrchardCore.Media.Events;
using OrchardCore.Media.Realtime;

namespace OrchardCore.Media.Hubs;

public class MediaSignalREventHandler : IMediaEventHandler
{
    private readonly IHubContext<MediaHub> _hubContext;
    private readonly MediaChangeEventFactory _eventFactory;

    public MediaSignalREventHandler(IHubContext<MediaHub> hubContext, MediaChangeEventFactory eventFactory)
    {
        _hubContext = hubContext;
        _eventFactory = eventFactory;
    }

    public Task MediaDeletedFileAsync(MediaDeletedContext context)
        => SendAsync("fileDeleted", context.Path, includeItem: false);

    public Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
        => SendAsync("directoryDeleted", context.Path, includeItem: false);

    public Task MediaMovedAsync(MediaMoveContext context)
        => SendAsync("fileMoved", context.OldPath, context.NewPath, includeItem: true);

    public Task MediaCreatedDirectoryAsync(MediaCreatedContext context)
        => SendAsync("directoryCreated", context.Path, includeItem: false);

    public Task MediaCreatedFileAsync(MediaCreatedContext context)
        => SendAsync("fileUploaded", context.Path, includeItem: true);

    public Task MediaCopiedFileAsync(MediaMoveContext context)
        => SendAsync("fileCopied", context.OldPath, context.NewPath, includeItem: true);

    private Task SendAsync(string action, string path, bool includeItem)
        => SendAsync(action, path, newPath: null, includeItem);

    private async Task SendAsync(string action, string path, string newPath, bool includeItem)
    {
        // The payload carries the affected entry so clients patch their store instead of each reloading
        // the directory. It is resolved once here, not once per connected client.
        var message = await _eventFactory.CreateAsync(action, path, newPath, includeItem);

        var group = FolderGroup(path);

        if (newPath is null)
        {
            await _hubContext.Clients.Group(group).SendAsync("MediaChanged", message);

            return;
        }

        var newGroup = FolderGroup(newPath);

        // If both paths share the same parent directory, avoid sending a duplicate notification.
        if (group == newGroup)
        {
            await _hubContext.Clients.Group(group).SendAsync("MediaChanged", message);

            return;
        }

        await _hubContext.Clients.Groups(group, newGroup).SendAsync("MediaChanged", message);
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
