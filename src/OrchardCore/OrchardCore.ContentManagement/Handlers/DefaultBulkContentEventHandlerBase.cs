using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;

public class DefaultBulkContentEventHandlerBase : IBulkContentEventHandler
{
    public virtual Task ImportingAsync(IEnumerable<ImportContentContext> contexts) => Task.CompletedTask;
    public virtual Task ImportedAsync(IEnumerable<ImportContentContext> contexts) => Task.CompletedTask;
}
