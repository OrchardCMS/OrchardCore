using System.Threading.Tasks;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Events;

internal class SecureMediaFileStoreEventHandler : MediaEventHandlerBase
{
    private readonly SecureMediaDirectoryChangeHelper _changeHelper;

    public SecureMediaFileStoreEventHandler(SecureMediaDirectoryChangeHelper changeHelper)
    {
        _changeHelper = changeHelper;
    }

    public override Task MediaCreatedDirectoryAsync(MediaCreatedContext context)
    {
        return context.Result ? _changeHelper.UpdateAsync() : Task.CompletedTask;
    }

    public override Task MediaDeletedDirectoryAsync(MediaDeletedContext context)
    {
        return context.Result ? _changeHelper.UpdateAsync() : Task.CompletedTask;
    }
}
