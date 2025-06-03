using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.Core.Handlers;

public abstract class IndexEntityHandlerBase : ModelHandlerBase<IndexEntity>, IIndexEntityHandler
{
    public virtual Task ExportingAsync(IndexEntityExportingContext context)
        => Task.CompletedTask;

    public virtual Task ResetAsync(IndexEntityResetContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedAsync(IndexEntitySynchronizedContext context)
        => Task.CompletedTask;
}
