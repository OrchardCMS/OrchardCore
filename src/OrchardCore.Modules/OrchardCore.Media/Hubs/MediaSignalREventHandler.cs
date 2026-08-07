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

    public async Task MediaDeletedFileAsync(MediaDeletedContext context)
        => await SendAsync("fileDeleted", context.Path, includeItem: false);

    public async Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
        => await SendAsync("directoryDeleted", context.Path, includeItem: false);

    public async Task MediaMovedAsync(MediaMoveContext context)
        => await SendAsync("fileMoved", context.OldPath, context.NewPath, includeItem: true);

    public async Task MediaCreatedDirectoryAsync(MediaCreatedContext context)
        => await SendAsync("directoryCreated", context.Path, includeItem: false);

    public async Task MediaCreatedFileAsync(MediaCreatedContext context)
        => await SendAsync("fileUploaded", context.Path, includeItem: true);

    public async Task MediaCopiedFileAsync(MediaMoveContext context)
        => await SendAsync("fileCopied", context.OldPath, context.NewPath, includeItem: true);

    private async Task SendAsync(string action, string path, bool includeItem)
        => await SendAsync(action, path, newPath: null, includeItem);

    private async Task SendAsync(string action, string path, string newPath, bool includeItem)
    {
        var message = await _eventFactory.CreateAsync(action, path, newPath, includeItem);

        await _hubContext.Clients.All.SendAsync("MediaChanged", message);
    }
}
