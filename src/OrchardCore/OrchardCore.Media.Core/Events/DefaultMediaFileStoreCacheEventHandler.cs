using OrchardCore.Media.Events;

namespace OrchardCore.Media.Core.Events;

public class DefaultMediaFileStoreCacheEventHandler : MediaEventHandlerBase
{
    private readonly IMediaFileStoreCacheFileProvider _mediaFileStoreCacheFileProvider;

    public DefaultMediaFileStoreCacheEventHandler(IMediaFileStoreCacheFileProvider mediaFileStoreCacheFileProvider)
    {
        _mediaFileStoreCacheFileProvider = mediaFileStoreCacheFileProvider;
    }

    public override Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
        => _mediaFileStoreCacheFileProvider.TryDeleteDirectoryAsync(context.Path);

    public override Task MediaDeletedFileAsync(MediaDeletedContext context)
        => _mediaFileStoreCacheFileProvider.TryDeleteFileAsync(context.Path);

    public override Task MediaMovedAsync(MediaMoveContext context)
        => _mediaFileStoreCacheFileProvider.TryDeleteFileAsync(context.OldPath);
}
