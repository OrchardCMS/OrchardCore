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

    public async Task MediaDeletedFileAsync(MediaDeletedContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "fileDeleted",
            path = context.Path,
        });
    }

    public async Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "directoryDeleted",
            path = context.Path,
        });
    }

    public async Task MediaMovedAsync(MediaMoveContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "fileMoved",
            path = context.OldPath,
            newPath = context.NewPath,
        });
    }

    public async Task MediaCreatedDirectoryAsync(MediaCreatedContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "directoryCreated",
            path = context.Path,
        });
    }

    public async Task MediaCreatedFileAsync(MediaCreatedContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "fileUploaded",
            path = context.Path,
        });
    }

    public async Task MediaCopiedFileAsync(MediaMoveContext context)
    {
        await _hubContext.Clients.All.SendAsync("MediaChanged", new
        {
            action = "fileCopied",
            path = context.OldPath,
            newPath = context.NewPath,
        });
    }
}
