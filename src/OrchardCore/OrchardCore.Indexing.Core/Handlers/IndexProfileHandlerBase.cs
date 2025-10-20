using OrchardCore.Catalogs;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core.Handlers;

public abstract class IndexProfileHandlerBase : CatalogEntryHandlerBase<IndexProfile>, IIndexProfileHandler
{
    public virtual Task ExportingAsync(IndexProfileExportingContext context)
        => Task.CompletedTask;

    public virtual Task ResetAsync(IndexProfileResetContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedAsync(IndexProfileSynchronizedContext context)
        => Task.CompletedTask;
}
