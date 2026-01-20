using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.Core.Handlers;

public abstract class IndexProfileHandlerBase : ModelHandlerBase<IndexProfile>, IIndexProfileHandler
{
    public virtual Task ExportingAsync(IndexProfileExportingContext context)
        => Task.CompletedTask;

    public virtual Task ResetAsync(IndexProfileResetContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedAsync(IndexProfileSynchronizedContext context)
        => Task.CompletedTask;
}
